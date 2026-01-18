using DigiStore.Framework.Endpoints;
using DigiStore.Framework.Proxies;
using DigiStore.TgBot.Application.Handlers.Adstracts;
using DigiStore.TgBot.Application.Interfaces.Services;
using DigiStore.TgBot.Application.Options;
using DigiStore.TgBot.Application.Services;
using DigiStore.TgBot.Application.Updates;
using DigiStore.TgBot.Application.Updates.Dispatchers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Telegram.Bot;


namespace DigiStore.TgBot.Application;

public static class DependencyInjectionExtensions
{
	public static IServiceCollection AddTgBotApplication(this IServiceCollection services, IConfiguration configuration)
	{
		services.AddEndpoints(typeof(DependencyInjectionExtensions).Assembly);

		services.Configure<ServiceOptions>(configuration.GetSection(nameof(ServiceOptions)));
		services.Configure<TelegramOptions>(configuration.GetSection(nameof(TelegramOptions)));
		//// Bind ServiceOptions to the DI system so it can be resolved via IOptions<ServiceOptions>
		//services.Configure<ServiceOptions>(configuration);

		services.AddHttpClient("TelegramBotProxyClient")
			.ConfigurePrimaryHttpMessageHandler(sp =>
			{
				var telegramOptions = sp.GetRequiredService<IOptions<TelegramOptions>>().Value;
				var proxyTg = telegramOptions.Proxy;

				return HProxy.GetClientHandler(proxyTg);
			});


		services.AddScoped<ITelegramBotClient>(x =>
		{
			var telegramOptions = x.GetRequiredService<IOptions<TelegramOptions>>().Value;
			var token = telegramOptions.BotToken;

			if (!String.IsNullOrWhiteSpace(telegramOptions.Proxy))
			{
				var clientFactory = x.GetRequiredService<IHttpClientFactory>();
				var client = clientFactory.CreateClient("TelegramBotProxyClient");
				return new TelegramBotClient(token, client);
			}

			return new TelegramBotClient(token);
		});


        // WalletService depends on an IWalletHttpClient (not System.Net.Http.HttpClient),
        // so register it as a scoped service instead of a typed HttpClient.

		// Session & Localization
		services.AddScoped<IProfileService, ProfileService>();

		services.AddSingleton<HandlerCollections>();
		services.AddScoped<CommandDispatcher>();
		services.AddScoped<CallbackDispatcher>();
		services.AddScoped<InputMessageDispatcher>();
		services.AddScoped<IUpdateDispatcher>(sp => sp.GetRequiredService<CommandDispatcher>());
		services.AddScoped<IUpdateDispatcher>(sp => sp.GetRequiredService<CallbackDispatcher>());
		services.AddScoped<IUpdateDispatcher>(sp => sp.GetRequiredService<InputMessageDispatcher>());
		services.AddScoped<UpdatePipeline>();
		services.AddScoped<UpdateHandler>();

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
