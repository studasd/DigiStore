using CSharpFunctionalExtensions;
using DigiStore.SharedKernel;
using DigiStore.TgBot.Application.Handlers.Adstracts;
using DigiStore.TgBot.Application.Interfaces.Services;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Types;

namespace DigiStore.TgBot.Application.Updates.Dispatchers;

public sealed class InputMessageDispatcher : IUpdateDispatcher
{
	private readonly IServiceProvider _serviceProvider;
	private readonly ISessionService _sessionService;
	private readonly HandlerCollections _registry;
	private readonly ILogger<InputMessageDispatcher> _logger;

	public InputMessageDispatcher(
		IServiceProvider serviceProvider,
		ISessionService sessionService,
		HandlerCollections registry,
		ILogger<InputMessageDispatcher> logger)
	{
		_serviceProvider = serviceProvider;
		_sessionService = sessionService;
		_registry = registry;
		_logger = logger;
	}

	public bool CanHandle(Update update)
		=> update.Message?.Text != null && !update.Message.Text.StartsWith("/");

	public async Task<UnitResult<Error>> DispatchAsync(Update update, CancellationToken token = default)
	{
		var message = update.Message;
		if (message == null || message.From == null || message.Text == null)
			return Result.Success<Error>();

		var sessionResult = await _sessionService.GetSessionAsync(message.From.Id, token);
		if (sessionResult.IsFailure)
			return sessionResult.Error;

		var state = sessionResult.Value.CurrentState;
		if (string.IsNullOrWhiteSpace(state))
			return Result.Success<Error>();

		if (!_registry.InputMessageHandlers.TryGetValue(state, out var handlerType))
			return Result.Success<Error>();

		var handler = _serviceProvider.GetService(handlerType) as IInputMessageHandler;
		if (handler == null)
		{
			_logger.LogError("Failed to create handler instance for input message: {Type}", handlerType.Name);
			return Error.NotFound("handler.input", "Failed to create handler instance for input message");
		}

		return await handler.HandleAsync(message, token);
	}
}
