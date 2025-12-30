using Microsoft.AspNetCore.Http.HttpResults;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateSlimBuilder(args);

builder.Services.ConfigureHttpJsonOptions(options =>
{
	options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
});

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
	app.MapOpenApi();
}

Todo[] sampleTodos =
[
	new(1, "Walk the dog"),
	new(2, "Do the dishes", DateOnly.FromDateTime(DateTime.Now)),
	new(3, "Do the laundry", DateOnly.FromDateTime(DateTime.Now.AddDays(1))),
	new(4, "Clean the bathroom"),
	new(5, "Clean the car", DateOnly.FromDateTime(DateTime.Now.AddDays(2)))
];

var todosApi = app.MapGroup("/todos");
todosApi.MapGet("/", () => sampleTodos)
		.WithName("GetTodos");

todosApi.MapGet("/{id}", Results<Ok<Todo>, NotFound> (int id) =>
	sampleTodos.FirstOrDefault(a => a.Id == id) is { } todo
		? TypedResults.Ok(todo)
		: TypedResults.NotFound())
	.WithName("GetTodoById");

app.Run();

public record Todo(int Id, string? Title, DateOnly? DueBy = null, bool IsComplete = false);

[JsonSerializable(typeof(Todo[]))]
internal partial class AppJsonSerializerContext : JsonSerializerContext
{

}





//////// Web/Program.cs - TgBot

//////builder.Services.AddControllers();
//////builder.Services.AddTgBotServices(builder.Configuration);
//////builder.Services.AddLogging();

//////var app = builder.Build();

//////// Webhook setup
//////var botToken = builder.Configuration["Telegram:BotToken"];
//////var botClient = app.Services.GetRequiredService<ITelegramBotClient>();
//////var webhookUrl = builder.Configuration["Telegram:WebhookUrl"];

//////if (!string.IsNullOrEmpty(webhookUrl))
//////{
//////	await botClient.SetWebhook(
//////		$"{webhookUrl}/telegram/webhook",
//////		allowedUpdates: Array.Empty<UpdateType>());
//////}

//////// Telegram update handler
//////app.MapPost("/telegram/webhook", async (Update update, ITelegramBotClient client, StartHandler startHandler, CallbackQueryHandler callbackHandler, CancellationToken ct) =>
//////{
//////	try
//////	{
//////		if (update.Message?.Text == "/start")
//////		{
//////			await startHandler.Handle(client, update, ct);
//////		}
//////		else if (update.CallbackQuery != null)
//////		{
//////			await callbackHandler.Handle(client, update, ct);
//////		}
//////	}
//////	catch (Exception ex)
//////	{
//////		app.Logger.LogError(ex, "Error processing Telegram update");
//////	}

//////	return Results.Ok();
//////});

//////app.Run();






//////builder.Services.AddControllers();
//////builder.Services.AddTgBotServices(builder.Configuration);
//////builder.Services.AddLogging(config =>
//////{
//////	config.AddConsole();
//////	config.AddDebug();
//////});
//////var app = builder.Build();
//////// Initialize bot commands
//////var botClient = app.Services.GetRequiredService<ITelegramBotClient>();
//////var commands = new BotCommand[]
//////{
//////new() { Command = "start", Description = "Start the bot" },
//////new() { Command = "profile", Description = "Show your profile" },
//////new() { Command = "balance", Description = "Check your balance" },
//////new() { Command = "language", Description = "Change language" },
//////new() { Command = "catalog", Description = "Browse catalog" },
//////new() { Command = "orders", Description = "View your orders" },
//////new() { Command = "help", Description = "Get help" },
//////};
//////try
//////{
//////	await botClient.SetMyCommands(commands);
//////	app.Logger.LogInformation("Bot commands set successfully");
//////}
//////catch (Exception ex)
//////{
//////	app.Logger.LogError(ex, "Failed to set bot commands");
//////}
//////// Configure webhook or polling
//////var webhookUrl = builder.Configuration["Telegram:WebhookUrl"];
//////if (!string.IsNullOrEmpty(webhookUrl))
//////{
//////	// Webhook mode
//////	await botClient.SetWebhook(
//////	$"{webhookUrl}/telegram/webhook",
//////	allowedUpdates: Array.Empty<UpdateType>());
//////	app.Logger.LogInformation("Webhook configured: {WebhookUrl}", webhookUrl);
//////	// Handle webhook updates
//////	app.MapPost("/telegram/webhook", HandleUpdateAsync);
//////}
//////else
//////{
//////	// Polling mode
//////	app.Logger.LogInformation("Starting polling mode");
//////	var cts = new CancellationTokenSource();
//////	_ = Task.Run(async () => await StartPollingAsync(app.Services, cts.Token), cts.Token)
//////app.Lifetime.ApplicationStopping.Register(() => cts.Cancel());
//////}
//////app.Run();
//////// Handle webhook updates
//////async Task HandleUpdateAsync(Update update, IServiceProvider services, CancellationToken ct = default)
//////{
//////	try
//////	{
//////		var botClient = services.GetRequiredService<ITelegramBotClient>();
//////		var commandHandler = services.GetRequiredService<CommandHandler>();
//////		var callbackHandler = services.GetRequiredService<CallbackQueryHandler>();
//////		if (update.Message?.Text != null)
//////		{
//////			var message = update.Message;
//////			switch (message.Text)
//////			{
//////				case BotCommands.Start:
//////					await commandHandler.HandleStartCommand(botClient, message, ct);
//////					break;
//////				case BotCommands.Profile:
//////					await commandHandler.HandleProfileCommand(botClient, message, ct);
//////					break;
//////				case BotCommands.Language:
//////					await commandHandler.HandleLanguageCommand(botClient, message, ct);
//////					break;
//////				case BotCommands.Balance:
//////					await commandHandler.HandleBalanceCommand(botClient, message, ct);
//////					break;
//////				default:
//////					break;
//////			}
//////		}
//////		else if (update.CallbackQuery != null)
//////		{
//////			await callbackHandler.Handle(botClient, update, ct);
//////		}
//////	}
//////	catch (Exception ex)
//////	{
//////		app.Logger.LogError(ex, "Error processing update");
//////	}
//////}
//////// Polling mode
//////async Task StartPollingAsync(IServiceProvider services, CancellationToken ct)
//////{
//////	var botClient = services.GetRequiredService<ITelegramBotClient>();
//////	var commandHandler = services.GetRequiredService<CommandHandler>();
//////	var callbackHandler = services.GetRequiredService<CallbackQueryHandler>();
//////	var logger = services.GetRequiredService<ILogger<Program>>();
//////	var receiverOptions = new ReceiverOptions
//////	{
//////		AllowedUpdates = Array.Empty<UpdateType>(),
//////		ThrowPendingUpdates = true
//////	};
//////	await botClient.DeleteWebhook(cancellationToken: ct);
//////	using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
//////	await botClient.ReceiveAsync(
//////	handleUpdateAsync: async (botClient, update, cancellationToken) =>
//////	{
//////		try
//////		{
//////			if (update.Message?.Text != null)
//////			{
//////				var message = update.Message;
//////				switch (message.Text)
//////				{
//////					case BotCommands.Start:
//////						await commandHandler.HandleStartCommand(botClient, message, cts);
//////						break;
//////					case BotCommands.Profile:
//////						await commandHandler.HandleProfileCommand(botClient, message, cts);
//////						break;
//////					case BotCommands.Language:
//////						await commandHandler.HandleLanguageCommand(botClient, message, cts);
//////						break;
//////					case BotCommands.Balance:
//////						await commandHandler.HandleBalanceCommand(botClient, message, cts);
//////						break;
//////					default:
//////						break;
//////				}
//////			}
//////			else if (update.CallbackQuery != null)
//////			{
//////				await callbackHandler.Handle(botClient, update, cancellationToken);
//////			}
//////		}
//////		catch (Exception ex)
//////		{
//////			logger.LogError(ex, "Error processing update");
//////		}
//////	},
//////handleErrorAsync: (botClient, exception, cancellationToken) =>
//////{
//////	logger.LogError(exception, "Polling error");
//////	return Task.CompletedTask;
//////},
//////receiverOptions: receiverOptions,
//////cancellationToken: cts.Token);
//////}
