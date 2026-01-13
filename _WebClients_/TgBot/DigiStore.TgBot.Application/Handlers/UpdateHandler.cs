using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Reflection;
using Telegram.Bot.Types;

using DigiStore.TgBot.Application.Interfaces.Services;
using DigiStore.TgBot.Application.Interfaces.Repositories;
using DigiStore.TgBot.Domain;
using DigiStore.TgBot.Domain.ValueObjects;

namespace DigiStore.TgBot.Application.Handlers;

/// <summary>
/// Универсальный обработчик Update, который автоматически находит и вызывает нужный хэндлер
/// </summary>
public class UpdateHandler
{
	private readonly IServiceScopeFactory _serviceScopeFactory;
	private readonly ILogger<UpdateHandler> _logger;
	private readonly ConcurrentDictionary<string, Type> _commandHandlers = new();
	private readonly ConcurrentDictionary<string, Type> _callbackHandlers = new();
	private readonly ConcurrentDictionary<string, Type> _callbackPrefixHandlers = new();

	public UpdateHandler(IServiceScopeFactory serviceScopeFactory, ILogger<UpdateHandler> logger)
	{
		_serviceScopeFactory = serviceScopeFactory;
		_logger = logger;
		InitializeHandlers();
	}

	/// <summary>
	/// Инициализирует словари хэндлеров на основе констант в хэндлерах
	/// </summary>
	private void InitializeHandlers()
	{
		// Получаем сборку Application, где находятся хэндлеры
		var assembly = typeof(ICommandHandler).Assembly;
		var handlerTypes = assembly.GetTypes()
			.Where(t => !t.IsAbstract && !t.IsInterface);

		foreach (var handlerType in handlerTypes)
		{
			// Регистрация обработчиков команд
			if (typeof(ICommandHandler).IsAssignableFrom(handlerType))
			{
				// Получаем константу Command из типа
				var commandField = handlerType.GetField("Command", BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
				if (commandField != null && commandField.IsLiteral && !commandField.IsInitOnly)
				{
					var command = commandField.GetValue(null)?.ToString();
					if (!string.IsNullOrEmpty(command))
					{
						_commandHandlers[command.ToLowerInvariant()] = handlerType;
						_logger.LogInformation("Registered command handler: {Command} -> {HandlerType}",
							command, handlerType.Name);
					}
				}
			}

			// Регистрация обработчиков колбэков
			if (typeof(ICallbackQueryHandler).IsAssignableFrom(handlerType))
			{
				// Получаем константы CallbackData и IsPrefix из типа
				var callbackDataField = handlerType.GetField("CallbackData", BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
				var isPrefixField = handlerType.GetField("IsPrefix", BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);

				if (callbackDataField != null && callbackDataField.IsLiteral && !callbackDataField.IsInitOnly)
				{
					var callbackData = callbackDataField.GetValue(null)?.ToString();
					if (!string.IsNullOrEmpty(callbackData))
					{
						var isPrefix = false;
						if (isPrefixField != null && isPrefixField.IsLiteral && !isPrefixField.IsInitOnly)
						{
							isPrefix = (bool)(isPrefixField.GetValue(null) ?? false);
						}

						if (isPrefix)
						{
							_callbackPrefixHandlers[callbackData] = handlerType;
							_logger.LogInformation("Registered callback prefix handler: {Prefix} -> {HandlerType}",
								callbackData, handlerType.Name);
						}
						else
						{
							_callbackHandlers[callbackData] = handlerType;
							_logger.LogInformation("Registered callback handler: {CallbackData} -> {HandlerType}",
								callbackData, handlerType.Name);
						}
					}
				}
			}
		}
	}

	/// <summary>
	/// Обрабатывает Update
	/// </summary>
	public async Task HandleUpdateAsync(Update update, CancellationToken cancellationToken = default)
	{
		// Создаем scope для каждого update, чтобы хэндлеры были scoped
		using var scope = _serviceScopeFactory.CreateScope();
		var serviceProvider = scope.ServiceProvider;

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
				try
				{
					var sessionService = serviceProvider.GetService<ISessionService>();
					var userService = serviceProvider.GetService<ITgUserService>();
					var userRepository = serviceProvider.GetService<ITgUserRepository>();

					if (sessionService != null && userService != null)
					{
						var session = await sessionService.GetOrCreateSessionAsync(telegramId.Value, cancellationToken);

						if (session.UserId == default)
						{
							var lang = session.LangCode;
							var userResult = await userService.GetOrCreateUserAsync(telegramId.Value, username, firstName, lastName, lang, cancellationToken);
							if (userResult.IsSuccess)
							{
								var dto = userResult.Value!;

								// Persist local TgUser mapping if repository available
								if (userRepository != null)
								{
									var tgUser = new TgUser
									{
										Id = Guid.NewGuid(),
										TelegramId = dto.TelegramId,
										UserId = dto.Id,
										FirstName = firstName ?? string.Empty,
										LastName = lastName ?? string.Empty,
										Username = username,
										IsActive = dto.IsActive,
										CreatedAt = DateTime.UtcNow,
										UpdatedAt = DateTime.UtcNow
									};

									await userRepository.AddOrUpdateAsync(tgUser, cancellationToken);
								}

								// Set session.UserId and optionally cache profile
								session.UserId = dto.Id;
								session.CachedProfile = new CachedUserProfileVO
								{
									UserId = dto.Id,
									TelegramId = dto.TelegramId,
									FirstName = dto.FullName?.Split(' ').FirstOrDefault() ?? string.Empty,
									LastName = dto.FullName?.Split(' ').LastOrDefault() ?? string.Empty,
									Username = dto.Username,
									LangCode = dto.LangCode,
									IsActive = dto.IsActive,
									Roles = dto.Roles,
								};

								await sessionService.UpdateSessionAsync(session, cancellationToken);

								// Notify UserService about activity
								_ = userService.UpdateActivityAsync(dto.Id, cancellationToken);
							}
							else
							{
								_logger.LogWarning("Failed to get or create user from UserService for TelegramId {TelegramId}: {Error}", telegramId.Value, userResult.Error?.GetMessage());
							}
						}
					}
				}
				catch (Exception ex)
				{
					_logger.LogError(ex, "Error ensuring user/session for TelegramId {TelegramId}", telegramId.Value);
				}
			}

			// Обработка команд
			if (update.Message?.Text != null && update.Message.Text.StartsWith("/"))
			{
				var command = update.Message.Text.Split(' ')[0].ToLowerInvariant();
				await HandleCommandAsync(update.Message, command, serviceProvider, cancellationToken);
				return;
			}

			// Обработка колбэков
			if (update.CallbackQuery != null)
			{
				await HandleCallbackQueryAsync(update.CallbackQuery, serviceProvider, cancellationToken);
				return;
			}


			// Record command history if session service available
			try
			{
				var sessionService = serviceProvider.GetService<ISessionService>();
				if (sessionService != null && update.Message.From != null)
				{
					await sessionService.RecordCommandAsync(update.Message.From.Id, null, update.Message?.Text, cancellationToken);
				}

				_logger.LogWarning("Unhandled update type: {UpdateType}", update.Type);
			}
			catch (Exception ex)
			{
				_logger.LogWarning(ex, "Failed to record command history for TelegramId {TelegramId}", update.Message.From?.Id);
			}

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
		IServiceProvider serviceProvider,
		CancellationToken cancellationToken)
	{
		if (!_commandHandlers.TryGetValue(command, out var handlerType))
		{
			_logger.LogWarning("No handler found for command: {Command}", command);
			return;
		}

		try
		{
			var handler = serviceProvider.GetService(handlerType) as ICommandHandler;
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
				var sessionService = serviceProvider.GetService<ISessionService>();
				if (sessionService != null && message.From != null)
				{
					await sessionService.RecordCommandAsync(message.From.Id, command, String.IsNullOrEmpty(command) ? message.Text : null, cancellationToken);
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
		IServiceProvider serviceProvider,
		CancellationToken cancellationToken)
	{
		if (callbackQuery.Data == null)
			return;

		var callbackData = callbackQuery.Data;

		// Сначала проверяем точное совпадение
		if (_callbackHandlers.TryGetValue(callbackData, out var exactHandlerType))
		{
			await ExecuteCallbackHandlerAsync(callbackQuery, exactHandlerType, serviceProvider, cancellationToken);
			// Record callback history
			try
			{
				var sessionService = serviceProvider.GetService<ISessionService>();
				if (sessionService != null && callbackQuery.From != null)
				{
					await sessionService.RecordCommandAsync(callbackQuery.From.Id, callbackData, String.IsNullOrEmpty(callbackData) ? callbackQuery.Message?.Text : null, cancellationToken);
				}
			}
			catch (Exception ex)
			{
				_logger.LogWarning(ex, "Failed to record callback history for TelegramId {TelegramId}", callbackQuery.From?.Id);
			}
			return;
		}

		// Затем проверяем префиксы
		foreach (var (prefix, handlerType) in _callbackPrefixHandlers)
		{
			if (callbackData.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
			{
				await ExecuteCallbackHandlerAsync(callbackQuery, handlerType, serviceProvider, cancellationToken);
				// Record callback history
				try
				{
					var sessionService = serviceProvider.GetService<ISessionService>();
					if (sessionService != null && callbackQuery.From != null)
					{
						await sessionService.RecordCommandAsync(callbackQuery.From.Id, callbackData, String.IsNullOrEmpty(callbackData) ? callbackQuery.Message?.Text : null, cancellationToken);
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
	}

	/// <summary>
	/// Выполняет обработчик колбэка
	/// </summary>
	private async Task ExecuteCallbackHandlerAsync(
		CallbackQuery callbackQuery,
		Type handlerType,
		IServiceProvider serviceProvider,
		CancellationToken cancellationToken)
	{
		try
		{
			var handler = serviceProvider.GetService(handlerType) as ICallbackQueryHandler;
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
