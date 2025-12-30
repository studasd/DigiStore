using DigiStore.TgBot.Application.Interfaces;
using DigiStore.TgBot.Application.Services;
using DigiStore.TgBot.Infrastructure;
using DigiStore.TgBot.Infrastructure.Handlers;
using Telegram.Bot;

namespace DigiStore.TgBot.Web;


public static class DependencyInjectionExtensions
{
	public static IServiceCollection AddTgBotServices(this IServiceCollection services, IConfiguration configuration)
	{
		// Telegram Bot Client
		var botToken = configuration["Telegram:BotToken"]
		?? throw new InvalidOperationException("Telegram BotToken not configured");
		services.AddScoped<ITelegramBotClient>(_ => new TelegramBotClient(botToken));
		// User & Wallet Services (HTTP clients)
		services.AddHttpClient<ITelegramUserService, TelegramUserService>()
		.ConfigureHttpClient(client =>
		{
			client.Timeout = TimeSpan.FromSeconds(10);
		});
		services.AddHttpClient<ITelegramWalletService, TelegramWalletService>()
		.ConfigureHttpClient(client =>
		{
			client.Timeout = TimeSpan.FromSeconds(10);
		});
		// Session & Localization
		services.AddScoped<ITelegramSessionService, TelegramSessionService>();
		services.AddScoped<ILocalizationService, LocalizationService>();
		services.AddScoped<ITelegramProfileService, TelegramProfileService>();
		
		// Handlers
		services.AddScoped<CommandHandler>();
		services.AddScoped<CallbackQueryHandler>();
		
		//// Redis
		//var redisConnection = configuration.GetConnectionString("Redis")
		//	?? throw new InvalidOperationException("Redis connection string not found");
		//var redis = ConnectionMultiplexer.Connect(redisConnection);
		//services.AddSingleton<IConnectionMultiplexer>(redis);
		
		return services;
	}
}