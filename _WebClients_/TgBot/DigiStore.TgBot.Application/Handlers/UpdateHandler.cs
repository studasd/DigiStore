using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Reflection;
using Telegram.Bot.Types;

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
			return;
		}

		// Затем проверяем префиксы
		foreach (var (prefix, handlerType) in _callbackPrefixHandlers)
		{
			if (callbackData.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
			{
				await ExecuteCallbackHandlerAsync(callbackQuery, handlerType, serviceProvider, cancellationToken);
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
