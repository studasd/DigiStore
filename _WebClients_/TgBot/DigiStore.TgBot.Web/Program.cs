using DigiStore.Framework.Endpoints;
using DigiStore.TgBot.Application.Services;
using DigiStore.TgBot.Application.Updates;
using DigiStore.TgBot.Infrastructure.Data;
using DigiStore.TgBot.Infrastructure.Data.Seeders;
using DigiStore.TgBot.Web;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
// Register hosted service for bot initialization and polling
builder.Services.AddHostedService<BotInitializerHostedService>();

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

app.MapPost("/telegram/webhook", async Task (
	[FromBody] Update update,
	[FromServices] UpdateHandler updateHandler,
	CancellationToken token) =>
		await updateHandler.HandleUpdateAsync(update, token));

app.Run();
