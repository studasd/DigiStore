using DigiStore.Framework.Proxies;
using DigiStore.TgBot.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;
using Telegram.Bot;

namespace DigiStore.TgBot.Infrastructure.BotAPI;

public static class DependencyInjectionExtensions
{
	public static IServiceCollection AddTgBotAPIInfrastructure(this IServiceCollection services, IConfiguration configuration)
	{
		services.Configure<TelegramOptions>(configuration.GetSection(nameof(TelegramOptions)));

		
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

		services.AddScoped<IBotAPIClient, BotAPIClient>();


		return services;
	}
}
