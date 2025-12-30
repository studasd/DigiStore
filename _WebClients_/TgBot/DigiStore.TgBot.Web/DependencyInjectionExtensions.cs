using DigiStore.TgBot.Application.Handlers;
using DigiStore.TgBot.Application.Handlers.Attributes;
using DigiStore.TgBot.Application.Interfaces;
using DigiStore.TgBot.Application.Services;
using DigiStore.TgBot.Infrastructure;
using System.Reflection;
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
		
		// Автоматическая регистрация всех хэндлеров команд и колбэков
		RegisterHandlers(services);
		
		// UpdateHandler (singleton, так как инициализирует словари при создании, использует IServiceScopeFactory для scope)
		services.AddSingleton<UpdateHandler>();
		
		//// Redis
		//var redisConnection = configuration.GetConnectionString("Redis")
		//	?? throw new InvalidOperationException("Redis connection string not found");
		//var redis = ConnectionMultiplexer.Connect(redisConnection);
		//services.AddSingleton<IConnectionMultiplexer>(redis);
		
		return services;
	}

	/// <summary>
	/// Автоматически регистрирует все хэндлеры команд и колбэков в DI
	/// </summary>
	private static void RegisterHandlers(IServiceCollection services)
	{
		// Получаем сборку Application, где находятся хэндлеры
		var applicationAssembly = typeof(ICommandHandler).Assembly;
		
		var handlerTypes = applicationAssembly.GetTypes()
			.Where(t => !t.IsAbstract && !t.IsInterface)
			.Where(t => t.GetCustomAttribute<CommandAttribute>() != null || 
						t.GetCustomAttribute<CallbackQueryAttribute>() != null);

		foreach (var handlerType in handlerTypes)
		{
			services.AddScoped(handlerType);
		}
	}
}