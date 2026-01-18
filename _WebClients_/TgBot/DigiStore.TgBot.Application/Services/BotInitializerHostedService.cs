using DigiStore.TgBot.Application.Handlers;
using DigiStore.TgBot.Application.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Telegram.Bot;
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

        var botClient = sp.GetRequiredService<ITelegramBotClient>();
        var telegramOptions = sp.GetRequiredService<IOptions<TelegramOptions>>().Value;

        // Set Commands
        try
        {
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

			await botClient.SetMyCommands(commands, cancellationToken: stoppingToken);
            _logger.LogInformation("Bot commands set successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to set bot commands");
        }


        // Set Webhook
        if (!string.IsNullOrEmpty(telegramOptions.WebhookUrl) && telegramOptions.IsWebhook)
        {
            try
            {
                await botClient.SetWebhook(
                    $"{telegramOptions.WebhookUrl}",
                    allowedUpdates: Array.Empty<UpdateType>(),
					cancellationToken: stoppingToken);

                _logger.LogInformation("Webhook configured: {WebhookUrl}", telegramOptions.WebhookUrl);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to set webhook");
            }

            // Hosted webhook mode: nothing else to do here - ASP.NET will receive POSTs at the mapped endpoint
            return;
        }



		// Запуск polling режима

		_logger.LogInformation("Starting polling mode from hosted service");

        try
        {
            await botClient.DeleteWebhook(cancellationToken: stoppingToken);

            var updateHandlerServ = sp.GetRequiredService<UpdateHandler>();
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
                        await updateHandlerServ.HandleUpdateAsync(update, token);
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
