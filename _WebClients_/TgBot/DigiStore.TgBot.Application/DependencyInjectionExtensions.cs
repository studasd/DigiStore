using DigiStore.TgBot.Application.Interfaces;
using DigiStore.TgBot.Application.Interfaces.Services;
using DigiStore.TgBot.Application.Options;
using DigiStore.TgBot.Application.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StudCoreKit.Framework.Endpoints;
using StudCoreKit.SharedKernel.Extensions;
using StudTgBotApi.Contracts.Interfaces;


namespace DigiStore.TgBot.Application;

public static class DependencyInjectionExtensions
{
	public static IServiceCollection AddTgBotApplication(this IServiceCollection services, IConfiguration configuration)
	{
		// Регистрируем все эндпоинты из текущей сборки
		services.AddEndpoints(typeof(DependencyInjectionExtensions).Assembly);
		// Регистрируем хэндлеры
		services.AddScopedFromInterface<ITgBotHandler>(typeof(DependencyInjectionExtensions).Assembly);

		services.Configure<ServiceOptions>(configuration.GetSection(nameof(ServiceOptions)));


		// Session & Localization
		services.AddScoped<ISessionService, SessionService>();
		services.AddScoped<ILocalizationService, LocalizationService>();
		services.AddScoped<IProfileService, ProfileService>();

		//services.AddScoped<ICallbackDataParser, CallbackDataParser>();

		//// Redis
		//var redisConnection = configuration.GetConnectionString("Redis")
		//	?? throw new InvalidOperationException("Redis connection string not found");
		//var redis = ConnectionMultiplexer.Connect(redisConnection);
		//services.AddSingleton<IConnectionMultiplexer>(redis);

		return services;
	}
}


public class DigiStoreApplication { }
