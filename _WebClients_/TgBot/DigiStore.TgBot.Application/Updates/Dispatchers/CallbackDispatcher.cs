using CSharpFunctionalExtensions;
using DigiStore.SharedKernel;
using DigiStore.TgBot.Application.Handlers.Adstracts;
using DigiStore.TgBot.Application.Interfaces.Services;
using DigiStore.TgBot.Application.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace DigiStore.TgBot.Application.Updates.Dispatchers;

public sealed class CallbackDispatcher : IUpdateDispatcher
{
	private readonly ITelegramBotClient _botClient;
	private readonly IServiceProvider _serviceProvider;
	private readonly ISessionService _sessionService;
	private readonly HandlerCollections _registry;
	private readonly TelegramOptions _tgOptions;
	private readonly ILogger<CallbackDispatcher> _logger;

	public CallbackDispatcher(
		ITelegramBotClient botClient,
		IServiceProvider serviceProvider,
		ISessionService sessionService,
		HandlerCollections registry,
		IOptions<TelegramOptions> options,
		ILogger<CallbackDispatcher> logger)
	{
		_botClient = botClient;
		_serviceProvider = serviceProvider;
		_sessionService = sessionService;
		_registry = registry;
		_tgOptions = options.Value;
		_logger = logger;
	}

	public bool CanHandle(Update update) => update.CallbackQuery != null;

	public async Task<UnitResult<Error>> DispatchAsync(Update update, CancellationToken token = default)
	{
		var callbackQuery = update.CallbackQuery;
		if (callbackQuery == null)
			return Result.Success<Error>();

		if (callbackQuery.Data == null)
			return Error.NotFound("handle.callback", "No data found for callbackQuery");

		if (_tgOptions.IsDebugShortResponse)
		{
			await _botClient.AnswerCallbackQuery(callbackQuery.Id, "DEBUG режим. Ответ получен.", cancellationToken: token);
		}

		var callbackData = callbackQuery.Data;

		if (_registry.CallbackHandlers.TryGetValue(callbackData, out var exactHandlerType))
		{
			return await ExecuteAsync(callbackQuery, exactHandlerType, token);
		}

		foreach (var kv in _registry.CallbackPrefixHandlers)
		{
			var prefix = kv.Key;
			var handlerType = kv.Value;

			if (callbackData.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
			{
				return await ExecuteAsync(callbackQuery, handlerType, token);
			}
		}

		_logger.LogWarning("No handler found for callback: {CallbackData}", callbackData);
		await _botClient.AnswerCallbackQuery(callbackQuery.Id, "Не реализовано", cancellationToken: token);
		return Result.Success<Error>();
	}

	private async Task<UnitResult<Error>> ExecuteAsync(CallbackQuery callbackQuery, Type handlerType, CancellationToken token)
	{
		var handler = _serviceProvider.GetService(handlerType) as ICallbackQueryHandler;
		if (handler == null)
		{
			_logger.LogError("Failed to create handler instance for callback: {Type}", handlerType.Name);
			return Error.NotFound("handler.callback", "Failed to create handler instance for callback");
		}

		var handlerResult = await handler.HandleAsync(callbackQuery, token);
		if (handlerResult.IsFailure)
			return handlerResult.Error;

		return Result.Success<Error>();
	}
}
