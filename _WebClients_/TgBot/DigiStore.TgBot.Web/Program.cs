using DigiStore.TgBot.Application.Handlers;
using DigiStore.TgBot.Web;
using Microsoft.AspNetCore.Http.Json;
using System.Text.Json.Serialization.Metadata;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Microsoft.Extensions.Options;
using DigiStore.TgBot.Application.Options;
using Microsoft.Extensions.DependencyInjection;
using DigiStore.TgBot.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);


// Гарантирует, что в runtime используется DefaultJsonTypeInfoResolver (reflection-based)
// и не произойдёт NotSupportedException при GetTypeInfo для типов, не зарегистрированных в OpenAPI контексте.
builder.Services.Configure<JsonOptions>(opts =>
{
	opts.SerializerOptions.TypeInfoResolver = new DefaultJsonTypeInfoResolver();
});

// Добавляем сервисы Telegram бота
builder.Services.AddControllers();
builder.Services.AddTgBotServices(builder.Configuration);

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Инициализация команд бота
using var scope = app.Services.CreateScope();
var serviceProvider = scope.ServiceProvider;

var dbContext = serviceProvider.GetRequiredService<TgBotDbContext>();
await dbContext.Database.MigrateAsync();


var botClient = serviceProvider.GetRequiredService<ITelegramBotClient>();
var telegramOptions = serviceProvider.GetRequiredService<IOptions<TelegramOptions>>().Value;

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

try
{
	await botClient.SetMyCommands(commands);
	app.Logger.LogInformation("Bot commands set successfully");
}
catch (Exception ex)
{
	app.Logger.LogError(ex, "Failed to set bot commands");
}

// Настройка webhook или polling
// var webhookUrl = builder.Configuration["Telegram:WebhookUrl"];
if (!string.IsNullOrEmpty(telegramOptions.WebhookUrl) && telegramOptions.IsWebhook)
{
	// Webhook mode
	await botClient.SetWebhook(
		$"{telegramOptions.WebhookUrl}/telegram/webhook",
		allowedUpdates: Array.Empty<UpdateType>());
	app.Logger.LogInformation("Webhook configured: {WebhookUrl}", telegramOptions.WebhookUrl);
	
	// Обработка webhook updates
	app.MapPost("/telegram/webhook", async (Update update, UpdateHandler updateHandler, CancellationToken ct) =>
	{
		try
		{
			await updateHandler.HandleUpdateAsync(update, ct);
		}
		catch (Exception ex)
		{
			app.Logger.LogError(ex, "Error processing Telegram update");
		}
		
		return Results.Ok();
	});
}
else
{
	// Polling mode
	app.Logger.LogInformation("Starting polling mode");
	var cts = new CancellationTokenSource();
	
	_ = Task.Run(async () => await StartPollingAsync(app.Services, cts.Token), cts.Token);
	app.Lifetime.ApplicationStopping.Register(() => cts.Cancel());
}

app.Run();

/// <summary>
/// Запуск polling режима
/// </summary>
async Task StartPollingAsync(IServiceProvider services, CancellationToken ct)
{
	using var scope = app.Services.CreateScope();

	var botClient = scope.ServiceProvider.GetRequiredService<ITelegramBotClient>();
	var updateHandler = scope.ServiceProvider.GetRequiredService<UpdateHandler>();
	var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
	
	var receiverOptions = new ReceiverOptions
	{
		AllowedUpdates = Array.Empty<UpdateType>(),
		//ThrowPendingUpdates = true
	};
	
	await botClient.DeleteWebhook(cancellationToken: ct);
	
	using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
	
	await botClient.ReceiveAsync(
		updateHandler: async (botClient, update, cancellationToken) =>
		{
			try
			{
				await updateHandler.HandleUpdateAsync(update, cancellationToken);
			}
			catch (Exception ex)
			{
				logger.LogError(ex, "Error processing update");
			}
		},
		errorHandler: (botClient, exception, cancellationToken) =>
		{
			logger.LogError(exception, "Polling error");
			return Task.CompletedTask;
		},
		receiverOptions: receiverOptions,
		cancellationToken: cts.Token);
}
