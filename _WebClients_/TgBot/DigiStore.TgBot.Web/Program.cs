using DigiStore.TgBot.Application.Services;
using DigiStore.TgBot.Infrastructure.Postgres.Data;
using DigiStore.TgBot.Infrastructure.Postgres.Data.Seeders;
using DigiStore.TgBot.Web;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using StudCoreKit.Framework.Endpoints;
using StudTgBotApi.Contracts.Options;
using StudTgBotApi.Interfaces;
using System.Text.Json.Serialization.Metadata;
using Telegram.Bot.Types;

var builder = WebApplication.CreateBuilder(args);


// Гарантирует, что в runtime используется DefaultJsonTypeInfoResolver (reflection-based)
// и не произойдёт NotSupportedException при GetTypeInfo для типов, не зарегистрированных в OpenAPI контексте.
builder.Services.Configure<Microsoft.AspNetCore.Http.Json.JsonOptions>(opts =>
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

//var dbContext = serviceProvider.GetRequiredService<TgBotDbContext>();
//await dbContext.Database.MigrateAsync();

var db = serviceProvider.GetRequiredService<TgBotDbContext>();
//// Применить миграции (асинхронно)
//await db.Database.MigrateAsync();
// Выполнить seeding (асинхронно)
var seeder = serviceProvider.GetRequiredService<IDataSeeder>();
await seeder.SeedAsync(db, serviceProvider, CancellationToken.None);

app.MapEndpoints();

app.MapPost("/telegram/webhook/{botId}", async Task<IResult>(
    [FromRoute] long botId,
    [FromBody] Update update,
	[FromServices] IUpdateHandler updateHandler,
    [FromServices] ILogger<Program>	logger,
    [FromServices] IOptions<TelegramMultiOptions> opts,
    [FromServices] IBotClientFactory botClientFactory,
    [FromServices] IBotContext botContext,
	[FromHeader(Name = "X-Telegram-Bot-Api-Secret-Token")] string? secretToken = null,
	CancellationToken token = default) =>
    {
        try
        {
			var multiOptions = opts.Value;

			// Найти конфиг бота по сегменту пути (username) либо по секрету
			var botConfig = multiOptions.Bots.FirstOrDefault(b => b.BotToken.StartsWith(botId.ToString()))
				?? multiOptions.Bots.FirstOrDefault(b => !string.IsNullOrEmpty(secretToken) && b.SecretToken == secretToken);

			if (botConfig == null)
			{
				logger.LogWarning("Bot config not found for path segment {Bot}", botId);
				return Results.NotFound();
			}

			// Валидация секретного токена если он задан в конфиге
			if (!string.IsNullOrEmpty(botConfig.SecretToken) && botConfig.SecretToken != secretToken)
			{
				logger.LogWarning("Invalid secret token for bot {Bot}", botConfig.Username);
				return Results.Unauthorized();
			}

			// ⭐ Получаем клиента и устанавливаем контекст
			var botClient = botClientFactory.GetBotClient(botId);
			botContext.SetContext(botId, botClient, botConfig, multiOptions.IsDebugShortResponse);

			logger.LogInformation("Bot context set for '{BotKey}'", botConfig.Username);

			// Передать update в общий сервис
			await updateHandler.HandleUpdateAsync(update);

			return Results.Ok();
		}
        catch
        {
            return Results.StatusCode(StatusCodes.Status500InternalServerError);
        }
    });

app.Run();
