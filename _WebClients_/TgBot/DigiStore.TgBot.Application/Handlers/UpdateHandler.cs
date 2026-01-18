using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Reflection;
using Telegram.Bot.Types;

using DigiStore.TgBot.Application.Interfaces.Services;
using DigiStore.TgBot.Application.Interfaces.Repositories;
using DigiStore.TgBot.Domain;
using DigiStore.TgBot.Domain.ValueObjects;
using Telegram.Bot;
using DigiStore.Framework.Endpoints;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Builder;
using DigiStore.TgBot.Application.Handlers.Adstracts;
using CSharpFunctionalExtensions;
using DigiStore.SharedKernel;
using Microsoft.Extensions.Options;
using DigiStore.TgBot.Application.Options;

namespace DigiStore.TgBot.Application.Handlers;




/// <summary>
/// Универсальный обработчик Update, который автоматически находит и вызывает нужный хэндлер
/// </summary>
public class UpdateHandler
{
	private readonly ITelegramBotClient _botClient;
	private readonly ISessionService _sessionService;
	private readonly ITgUserService _userService;
	private readonly ITgUserRepository _userRepository;
	private readonly IServiceProvider _serviceProvider;
    private readonly TelegramOptions _tgOptions;
    private readonly ILogger<UpdateHandler> _logger;
    private readonly HandlerCollections _registry;

	public UpdateHandler(
		ITelegramBotClient botClient,
		ISessionService sessionService,
		ITgUserService userService,
		ITgUserRepository userRepository,
		IServiceProvider serviceProvider,
		IOptions<TelegramOptions> options,
		ILogger<UpdateHandler> logger,
        HandlerCollections registry)
    {
		_botClient = botClient;
		_sessionService = sessionService;
		_userService = userService;
		_userRepository = userRepository;
		_serviceProvider = serviceProvider;
		_tgOptions = options.Value;
        _logger = logger;
        _registry = registry;
    }


	/// <summary>
	/// Обрабатывает Update
	/// </summary>
	public async Task HandleUpdateAsync(Update update, CancellationToken token = default)
	{
		//if (_tgOptions.IsDebugShortResponse)
		//{
		//	// ✅ ОТВЕТИТЬ ТЕЛЕГРАМУ СРАЗУ
		//	// ✅ ОБРАБОТКА В ФОНЕ БЕЗ ОЖИДАНИЯ
		//	_ = ProcessUpdateAsync(update, token);
		//}
		//else
		{
			await ProcessUpdateAsync(update, token);
		}
	}


	private async Task ProcessUpdateAsync(Update update, CancellationToken token)
	{
		try
		{
			// Before dispatching handlers ensure we have session and linked user
			long? telegramId = null;
			string? username = null;
			string? firstName = null;
			string? lastName = null;

			if (update.Message?.From != null)
			{
				telegramId = update.Message.From.Id;
				username = update.Message.From.Username;
				firstName = update.Message.From.FirstName;
				lastName = update.Message.From.LastName;
			}
			else if (update.CallbackQuery?.From != null)
			{
				telegramId = update.CallbackQuery.From.Id;
				username = update.CallbackQuery.From.Username;
				firstName = update.CallbackQuery.From.FirstName;
				lastName = update.CallbackQuery.From.LastName;
			}


			if (telegramId.HasValue)
			{
				var sessionResult = await _sessionService.GetOrCreateSessionAsync(telegramId.Value, token);
				if (sessionResult.IsFailure)
				{
					_logger.LogWarning("Failed to get or create user from Session for TelegramId {TelegramId}: {Error}", telegramId.Value, sessionResult.Error?.GetMessage());
					return;
				}

				var session = sessionResult.Value!;
				if (session.UserId == default)
				{
					var lang = session.LangCode;
					var userResult = await _userService.GetOrCreateUserAsync(telegramId.Value, username, firstName, lastName, lang, token);

					if (userResult.IsSuccess)
					{
						var userDto = userResult.Value!;

						var tgUser = new TgUser
						{
							Id = Guid.NewGuid(),
							TelegramId = userDto.TelegramId,
							UserId = userDto.Id,
							FirstName = firstName ?? string.Empty,
							LastName = lastName ?? string.Empty,
							Username = username,
							IsActive = userDto.IsActive,
							CreatedAt = DateTime.UtcNow,
							UpdatedAt = DateTime.UtcNow
						};

						await _userRepository.AddOrUpdateAsync(tgUser, token);


						// Set session.UserId and optionally cache profile
						session.UserId = userDto.Id;
						session.CachedProfile = new CachedUserProfileVO
						{
							UserId = userDto.Id,
							TelegramId = userDto.TelegramId,
							FirstName = userDto.FullName?.Split(' ').FirstOrDefault() ?? string.Empty,
							LastName = userDto.FullName?.Split(' ').LastOrDefault() ?? string.Empty,
							Username = userDto.Username,
							LangCode = userDto.LangCode,
							IsActive = userDto.IsActive,
							Roles = userDto.Roles,
						};

						await _sessionService.UpdateSessionAsync(session, token);

						// Notify UserService about activity
						_ = _userService.UpdateActivityAsync(userDto.Id, token);
					}
					else
					{
						_logger.LogWarning("Failed to get or create user from UserService for TelegramId {TelegramId}: {Error}", telegramId.Value, userResult.Error?.GetMessage());
					}
				}
			}

			// Обработка команд
			if (update.Message?.Text != null && update.Message.Text.StartsWith("/"))
			{
				var command = update.Message.Text.Split(' ')[0].ToLowerInvariant();
				var handleCommandResult = await HandleCommandAsync(update.Message, command, token);

				if (handleCommandResult.IsFailure)
				{
					_logger.LogError("Bad error HandleCommand: {errors}", handleCommandResult.Error.GetMessage());
				}
				return;
			}

			// Обработка колбэков
			if (update.CallbackQuery != null)
			{
				var handleCallbackQueryResult = await HandleCallbackQueryAsync(update.CallbackQuery, token);

				if (handleCallbackQueryResult.IsFailure)
				{
					_logger.LogError("Bad error HandleCallbackQuery: {errors}", handleCallbackQueryResult.Error.GetMessage());
					await _botClient.AnswerCallbackQuery(update.CallbackQuery.Id, "Не реализовано", cancellationToken: token);
				}
				return;
			}


			if (update.Message.From != null)
			{
				await _sessionService.RecordCommandAsync(update.Message.From.Id, null, update.Message?.Text, token);
			}

			_logger.LogWarning("Unhandled update type: {UpdateType}", update.Type);

		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error processing update");
		}
	}





	/// <summary>
	/// Обрабатывает команду
	/// </summary>
	private async Task<UnitResult<Error>> HandleCommandAsync(
		Message message,
		string command,
		CancellationToken token)
	{
        if (!_registry.CommandHandlers.TryGetValue(command, out var handlerType))
		{
			_logger.LogWarning("No handler found for command: {Command}", command);
			return Error.NotFound("handle.command", "No handler found for command");
		}

		var handler = _serviceProvider.GetService(handlerType) as ICommandHandler;
		if (handler == null)
		{
			_logger.LogError("Failed to create handler instance for command: {Command}, Type: {Type}",
				command, handlerType.Name);
			return Error.NotFound("handler.command", "Failed to create handler instance for command");
		}

		var handlerResult = await handler.HandleAsync(message, token);

		if(handlerResult.IsFailure)
		{
			return handlerResult.Error;
		}

		// Record command history if session service available
		if (message.From != null)
		{
			await _sessionService.RecordCommandAsync(message.From.Id, command, String.IsNullOrEmpty(command) ? message.Text : null, token);
		}

		return Result.Success<Error>();
	}

	/// <summary>
	/// Обрабатывает колбэк
	/// </summary>
	private async Task<UnitResult<Error>> HandleCallbackQueryAsync(
		CallbackQuery callbackQuery,
		CancellationToken token)
	{
		if (callbackQuery.Data == null)
			return Error.NotFound("handle.callback", "No data found for callbackQuery");


		if (_tgOptions.IsDebugShortResponse)
		{
			// ✅ ОТВЕТИТЬ ТЕЛЕГРАМУ СРАЗУ
			await _botClient.AnswerCallbackQuery(callbackQuery.Id, "DEBUG режим. Ответ получен.", cancellationToken: token);
		}
		


		var callbackData = callbackQuery.Data;

        // Сначала проверяем точное совпадение
        if (_registry.CallbackHandlers.TryGetValue(callbackData, out var exactHandlerType))
		{
			var executeResult = await ExecuteCallbackHandlerAsync(callbackQuery, exactHandlerType, token);

			if(executeResult.IsFailure)
				return executeResult.Error;
			
			if (callbackQuery.From != null)
			{
				await _sessionService.RecordCommandAsync(callbackQuery.From.Id, callbackData, String.IsNullOrEmpty(callbackData) ? callbackQuery.Message?.Text : null, token);
			}
			return Result.Success<Error>();
		}

		// Затем проверяем префиксы
		foreach (var (prefix, handlerType) in _registry.CallbackPrefixHandlers)
		{
			if (callbackData.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
			{
				var executeResult = await ExecuteCallbackHandlerAsync(callbackQuery, handlerType, token);
				
				if (executeResult.IsFailure)
					return executeResult.Error;

				if (callbackQuery.From != null)
				{
					await _sessionService.RecordCommandAsync(callbackQuery.From.Id, callbackData, String.IsNullOrEmpty(callbackData) ? callbackQuery.Message?.Text : null, token);
				}
				return Result.Success<Error>();
			}
		}

		_logger.LogWarning("No handler found for callback: {CallbackData}", callbackData);

		await _botClient.AnswerCallbackQuery(callbackQuery.Id, "Не реализовано", cancellationToken: token);

		return Result.Success<Error>();
	}

	/// <summary>
	/// Выполняет обработчик колбэка
	/// </summary>
	private async Task<UnitResult<Error>> ExecuteCallbackHandlerAsync(
		CallbackQuery callbackQuery,
		Type handlerType,
		CancellationToken token)
	{
		var handler = _serviceProvider.GetService(handlerType) as ICallbackQueryHandler;
		if (handler == null)
		{
			_logger.LogError("Failed to create handler instance for callback: {Type}", handlerType.Name);
			return Error.NotFound("handler.callback", "Failed to create handler instance for callback");
		}

		var handlerResult = await handler.HandleAsync(callbackQuery, token);

		if (handlerResult.IsFailure)
		{
			return handlerResult.Error;
		}

		return Result.Success<Error>();
	}
}
