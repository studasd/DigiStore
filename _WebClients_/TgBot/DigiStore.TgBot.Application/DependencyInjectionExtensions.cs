using DigiStore.Framework.Endpoints;
using DigiStore.SharedKernel.Extensions;
using DigiStore.TgBot.Application.Handlers.Adstracts;
using DigiStore.TgBot.Application.Interfaces;
using DigiStore.TgBot.Application.Interfaces.Services;
using DigiStore.TgBot.Application.Options;
using DigiStore.TgBot.Application.Services;
using DigiStore.TgBot.Application.Updates;
using DigiStore.TgBot.Application.Updates.Dispatchers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;


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

		services.AddSingleton<HandlerCollections>();
		services.AddScoped<CommandDispatcher>();
		services.AddScoped<CallbackDispatcher>();
		services.AddScoped<InputMessageDispatcher>();
		services.AddScoped<IUpdateDispatcher>(sp => sp.GetRequiredService<CommandDispatcher>());
		services.AddScoped<IUpdateDispatcher>(sp => sp.GetRequiredService<CallbackDispatcher>());
		services.AddScoped<IUpdateDispatcher>(sp => sp.GetRequiredService<InputMessageDispatcher>());
		services.AddScoped<UpdatePipeline>();
		services.AddScoped<UpdateHandler>();

		// Session & Localization
		services.AddScoped<ISessionService, SessionService>();
		services.AddScoped<ILocalizationService, LocalizationService>();
		services.AddScoped<IProfileService, ProfileService>();


		// Автоматическая регистрация всех хэндлеров команд и колбэков
		RegisterHandlers(services);


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
			.Where(t => typeof(ICommandHandler).IsAssignableFrom(t) || typeof(ICallbackQueryHandler).IsAssignableFrom(t) || typeof(IInputMessageHandler).IsAssignableFrom(t));

		foreach (var handlerType in handlerTypes)
		{
			services.AddScoped(handlerType);
		}
	}
}
