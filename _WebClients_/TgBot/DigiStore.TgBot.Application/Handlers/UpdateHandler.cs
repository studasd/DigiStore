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

namespace DigiStore.TgBot.Application.Handlers;


public sealed class ActivateUser : IEndpoint
{
	public void MapEndpoint(IEndpointRouteBuilder app)
	{
		app.MapPost("/telegram/webhook", async Task (
			[FromBody] Update update,
			[FromServices] UpdateHandler updateHandler,
			CancellationToken token) => 
				await updateHandler.HandleUpdateAsync(update, token));
	}
}



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
	private readonly ILogger<UpdateHandler> _logger;
    private readonly HandlerCollections _registry;

	public UpdateHandler(
		ITelegramBotClient botClient,
		ISessionService sessionService,
		ITgUserService userService,
		ITgUserRepository userRepository,
		IServiceProvider serviceProvider,
		ILogger<UpdateHandler> logger,
        HandlerCollections registry)
    {
		_botClient = botClient;
		_sessionService = sessionService;
		_userService = userService;
		_userRepository = userRepository;
		_serviceProvider = serviceProvider;
		_logger = logger;
        _registry = registry;
    }


	/// <summary>
	/// Обрабатывает Update
	/// </summary>
	public async Task HandleUpdateAsync(Update update, CancellationToken cancellationToken = default)
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
				var sessionResult = await _sessionService.GetOrCreateSessionAsync(telegramId.Value, cancellationToken);
				if (sessionResult.IsFailure)
				{
					_logger.LogWarning("Failed to get or create user from Session for TelegramId {TelegramId}: {Error}", telegramId.Value, sessionResult.Error?.GetMessage());
					return;
				}

				var session = sessionResult.Value!;
				if (session.UserId == default)
				{
					var lang = session.LangCode;
					var userResult = await _userService.GetOrCreateUserAsync(telegramId.Value, username, firstName, lastName, lang, cancellationToken);
						
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

						await _userRepository.AddOrUpdateAsync(tgUser, cancellationToken);


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

						await _sessionService.UpdateSessionAsync(session, cancellationToken);

						// Notify UserService about activity
						_ = _userService.UpdateActivityAsync(userDto.Id, cancellationToken);
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
                await HandleCommandAsync(update.Message, command, cancellationToken);
                return;
            }

			// Обработка колбэков
			if (update.CallbackQuery != null)
			{
				await HandleCallbackQueryAsync(update.CallbackQuery, cancellationToken);
				return;
			}


			if (update.Message.From != null)
			{
				await _sessionService.RecordCommandAsync(update.Message.From.Id, null, update.Message?.Text, cancellationToken);
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
	private async Task HandleCommandAsync(
		Message message,
		string command,
		CancellationToken cancellationToken)
	{
        if (!_registry.CommandHandlers.TryGetValue(command, out var handlerType))
		{
			_logger.LogWarning("No handler found for command: {Command}", command);
			return;
		}

		try
		{
			var handler = _serviceProvider.GetService(handlerType) as ICommandHandler;
			if (handler == null)
			{
				_logger.LogError("Failed to create handler instance for command: {Command}, Type: {Type}",
					command, handlerType.Name);
				return;
			}

			await handler.HandleAsync(message, cancellationToken);

			// Record command history if session service available
			try
			{
				if (message.From != null)
				{
					await _sessionService.RecordCommandAsync(message.From.Id, command, String.IsNullOrEmpty(command) ? message.Text : null, cancellationToken);
				}
			}
			catch (Exception ex)
			{
				_logger.LogWarning(ex, "Failed to record command history for TelegramId {TelegramId}", message.From?.Id);
			}
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error executing command handler: {Command}, Type: {Type}",
				command, handlerType.Name);
		}
	}

	/// <summary>
	/// Обрабатывает колбэк
	/// </summary>
	private async Task HandleCallbackQueryAsync(
		CallbackQuery callbackQuery,
		CancellationToken cancellationToken)
	{
		if (callbackQuery.Data == null)
			return;

		var callbackData = callbackQuery.Data;

        // Сначала проверяем точное совпадение
        if (_registry.CallbackHandlers.TryGetValue(callbackData, out var exactHandlerType))
		{
			await ExecuteCallbackHandlerAsync(callbackQuery, exactHandlerType, cancellationToken);
			// Record callback history
			try
			{
				if (callbackQuery.From != null)
				{
					await _sessionService.RecordCommandAsync(callbackQuery.From.Id, callbackData, String.IsNullOrEmpty(callbackData) ? callbackQuery.Message?.Text : null, cancellationToken);
				}
			}
			catch (Exception ex)
			{
				_logger.LogWarning(ex, "Failed to record callback history for TelegramId {TelegramId}", callbackQuery.From?.Id);
			}
			return;
		}

		// Затем проверяем префиксы
        foreach (var (prefix, handlerType) in _registry.CallbackPrefixHandlers)
		{
			if (callbackData.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
			{
				await ExecuteCallbackHandlerAsync(callbackQuery, handlerType, cancellationToken);
				// Record callback history
				try
				{
					if (callbackQuery.From != null)
					{
						await _sessionService.RecordCommandAsync(callbackQuery.From.Id, callbackData, String.IsNullOrEmpty(callbackData) ? callbackQuery.Message?.Text : null, cancellationToken);
					}
				}
				catch (Exception ex)
				{
					_logger.LogWarning(ex, "Failed to record callback history for TelegramId {TelegramId}", callbackQuery.From?.Id);
				}
				return;
			}
		}

		_logger.LogWarning("No handler found for callback: {CallbackData}", callbackData);

		await _botClient.AnswerCallbackQuery(callbackQuery.Id, "Не реализовано", cancellationToken: cancellationToken);
	}

	/// <summary>
	/// Выполняет обработчик колбэка
	/// </summary>
	private async Task ExecuteCallbackHandlerAsync(
		CallbackQuery callbackQuery,
		Type handlerType,
		CancellationToken cancellationToken)
	{
		try
		{
			var handler = _serviceProvider.GetService(handlerType) as ICallbackQueryHandler;
			if (handler == null)
			{
				_logger.LogError("Failed to create handler instance for callback: {Type}", handlerType.Name);
				return;
			}

			await handler.HandleAsync(callbackQuery, cancellationToken);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error executing callback handler: {Type}", handlerType.Name);
		}
	}
}
