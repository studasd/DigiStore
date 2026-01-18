using CSharpFunctionalExtensions;
using DigiStore.SharedKernel;
using DigiStore.TgBot.Application.Handlers.Adstracts;
using DigiStore.TgBot.Application.Interfaces.Services;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Types;

namespace DigiStore.TgBot.Application.Updates.Dispatchers;

public sealed class CommandDispatcher : IUpdateDispatcher
{
	private readonly IServiceProvider _serviceProvider;
	private readonly ISessionService _sessionService;
	private readonly HandlerCollections _registry;
	private readonly ILogger<CommandDispatcher> _logger;

	public CommandDispatcher(
		IServiceProvider serviceProvider,
		ISessionService sessionService,
		HandlerCollections registry,
		ILogger<CommandDispatcher> logger)
	{
		_serviceProvider = serviceProvider;
		_sessionService = sessionService;
		_registry = registry;
		_logger = logger;
	}

	public bool CanHandle(Update update)
		=> update.Message?.Text != null && update.Message.Text.StartsWith("/");

	public async Task<UnitResult<Error>> DispatchAsync(Update update, CancellationToken token = default)
	{
		var message = update.Message;
		if (message == null || string.IsNullOrWhiteSpace(message.Text) || !message.Text.StartsWith("/"))
			return Result.Success<Error>();

		var command = message.Text.Split(' ')[0].ToLowerInvariant();

		if (!_registry.CommandHandlers.TryGetValue(command, out var handlerType))
		{
			_logger.LogWarning("No handler found for command: {Command}", command);
			return Error.NotFound("handle.command", "No handler found for command");
		}

		var handler = _serviceProvider.GetService(handlerType) as ICommandHandler;
		if (handler == null)
		{
			_logger.LogError("Failed to create handler instance for command: {Command}, Type: {Type}", command, handlerType.Name);
			return Error.NotFound("handler.command", "Failed to create handler instance for command");
		}

		var handlerResult = await handler.HandleAsync(message, token);
		if (handlerResult.IsFailure)
			return handlerResult.Error;

		return Result.Success<Error>();
	}
}
