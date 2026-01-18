using DigiStore.TgBot.Application.Handlers.Adstracts;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Reflection;

namespace DigiStore.TgBot.Application.Handlers;

/// <summary>
/// Registry of handler types and lookup dictionaries.
/// Built once as a singleton to avoid re-scanning assemblies for each scoped UpdateHandler.
/// </summary>
public sealed class HandlerCollections
{
    public ConcurrentDictionary<string, Type> CommandHandlers { get; } = new();
    public ConcurrentDictionary<string, Type> CallbackHandlers { get; } = new();
    public ConcurrentDictionary<string, Type> CallbackPrefixHandlers { get; } = new();

	public ConcurrentDictionary<string, Type> InputMessageHandlers { get; } = new();



	public HandlerCollections(ILogger<HandlerCollections> logger)
    {
        InitializeHandlers(logger);
    }

    private void InitializeHandlers(ILogger logger)
    {
        var assembly = typeof(ICommandHandler).Assembly;
        var handlerTypes = assembly.GetTypes()
            .Where(t => !t.IsAbstract && !t.IsInterface);

        foreach (var handlerType in handlerTypes)
        {
            if (typeof(ICommandHandler).IsAssignableFrom(handlerType))
            {
                var commandField = handlerType.GetField("Command", BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
                if (commandField != null && commandField.IsLiteral && !commandField.IsInitOnly)
                {
                    var command = commandField.GetValue(null)?.ToString();
                    if (!string.IsNullOrEmpty(command))
                    {
                        CommandHandlers[command.ToLowerInvariant()] = handlerType;
                        logger.LogInformation("Registered command handler: {Command} -> {HandlerType}", command, handlerType.Name);
                    }
                }
            }
            
            if (typeof(IInputMessageHandler).IsAssignableFrom(handlerType))
            {
				// Для input-message хэндлеров используем ключом state (машинное состояние диалога)
				var stateField = handlerType.GetField("StateKey", BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
				if (stateField != null && stateField.IsLiteral && !stateField.IsInitOnly)
				{
					var state = stateField.GetValue(null)?.ToString();
					if (!string.IsNullOrWhiteSpace(state))
					{
						InputMessageHandlers[state] = handlerType;
						logger.LogInformation("Registered input message handler: {State} -> {HandlerType}", state, handlerType.Name);
					}
				}
			}

			if (typeof(ICallbackQueryHandler).IsAssignableFrom(handlerType))
            {
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
                            CallbackPrefixHandlers[callbackData] = handlerType;
                            logger.LogInformation("Registered callback prefix handler: {Prefix} -> {HandlerType}", callbackData, handlerType.Name);
                        }
                        else
                        {
                            CallbackHandlers[callbackData] = handlerType;
                            logger.LogInformation("Registered callback handler: {CallbackData} -> {HandlerType}", callbackData, handlerType.Name);
                        }
                    }
                }
            }
        }
    }
}
