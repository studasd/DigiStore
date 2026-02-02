using DigiStore.TgBot.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StudTgBotApi.Contracts.Interfaces;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace DigiStore.TgBot.Application.Services;

public class BotInitializerHostedService : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly ILogger<BotInitializerHostedService> _logger;

    public BotInitializerHostedService(IServiceProvider services, ILogger<BotInitializerHostedService> logger)
    {
        _services = services;
        _logger = logger;
    }


    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var scope = _services.CreateScope();
        var sp = scope.ServiceProvider;

        var botClient = sp.GetRequiredService<IBotAPIClient>();
        //var telegramOptions = sp.GetRequiredService<IOptions<TelegramOptions>>().Value;

        // Set Commands
		var commands = new BotCommand[]
		{
			new() { Command = "start", Description = "Start the bot" },
			new() { Command = "profile", Description = "Show your profile" },
			new() { Command = "balance", Description = "Check your balance" },
			new() { Command = "language", Description = "Change language" },
			new() { Command = "catalog", Description = "Browse catalog" },
			new() { Command = "orders", Description = "View your orders" },
			new() { Command = "help", Description = "Get help" },
		};

		var setCommandsResult = await botClient.SetMyCommandsAsync(commands, cancellationToken: stoppingToken);

        if (setCommandsResult.IsSuccess)
        {
			_logger.LogInformation("Bot commands set successfully");
		}


        // Set Webhook
        if (!string.IsNullOrEmpty(botClient.WebhookUrl) /*&& botClient.IsWebhook*/)
        {
            var setWebhookResult = await botClient.SetWebhookAsync(
                $"{botClient.WebhookUrl}",
                allowedUpdates: Array.Empty<UpdateType>(),
				cancellationToken: stoppingToken);

            if (setWebhookResult.IsFailure)
            {
				_logger.LogError("Failed to set webhook: {Message}", setWebhookResult.Error.GetMessage());
                return;
			}

            _logger.LogInformation("Webhook configured: {WebhookUrl}", botClient.WebhookUrl);

            // Hosted webhook mode: nothing else to do here - ASP.NET will receive POSTs at the mapped endpoint
            return;
        }



		// Запуск polling режима

		_logger.LogInformation("Starting polling mode from hosted service");

        try
        {
            var deleteResult = await botClient.DeleteWebhookAsync(cancellationToken: stoppingToken);
            if (deleteResult.IsFailure)
            {
                _logger.LogWarning("Failed to delete webhook: {Error}", deleteResult.Error);
			}

            var updateHandlerServ = sp.GetRequiredService<IUpdateHandler>();
            var logger = sp.GetRequiredService<ILogger<BotInitializerHostedService>>();

            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = Array.Empty<UpdateType>(), 
				//ThrowPendingUpdates = true
			};

            await botClient.ReceiveAsync(
                updateHandler: async (client, update, token) =>
                {
                    try
                    {
                        await updateHandlerServ.HandleUpdateAsync(client, update, token);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Error processing update");
                    }
                },
                errorHandler: (client, exception, token) =>
                {
                    logger.LogError(exception, "Polling error");
                    return Task.CompletedTask;
                },
                receiverOptions: receiverOptions,
                cancellationToken: stoppingToken);
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            // shutting down
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Polling terminated with error");
        }
    }
}
