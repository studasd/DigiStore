using DigiStore.TgBot.Application;
using DigiStore.TgBot.Application.Options;
using DigiStore.TgBot.Infrastructure.Postgres;
using Microsoft.Extensions.Options;
using StudCoreKit.Framework.Logging;
using System.Reflection;
using StudTgBotApi.Framework;
using DigiStore.TgBot.Application.Services;
using Telegram.Bot.Types;
using StudTgBotApi.Contracts.Interfaces;
using StudTgBotApi.Services;
using StudTgBotApi.Contracts.Enums;

namespace DigiStore.TgBot.Web;


public static class DependencyInjectionExtensions
{
	public static IServiceCollection AddTgBotServices(this IServiceCollection services, IConfiguration configuration)
	{
		//// Register hosted service for bot initialization and polling
		//services.AddHostedService<BotInitializerHostedService>();

		var commands = new BotCommand[]
		{
			new() { Command = "start", Description = "Start the bot" },
			new() { Command = "profile", Description = "Show your profile" },
			new() { Command = "balance", Description = "Check your balance" },
			//new() { Command = "language", Description = "Change language" },
			//new() { Command = "catalog", Description = "Browse catalog" },
			//new() { Command = "orders", Description = "View your orders" },
			//new() { Command = "help", Description = "Get help" },
		};

		services.AddScoped<IBotCommandProvider>(sp => new StaticBotCommandProvider(commands));


		services
			.AddTgBotApplication(configuration)
			.AddTgBotInfrastructurePostgres(configuration)
			.AddStudTgBotApi<DigiStoreApplication>(configuration)
			.AddSerilogLogging("TgBot", Assembly.GetExecutingAssembly(), sp => sp.GetRequiredService<IOptions<ServiceOptions>>().Value?.SeqUrl);

		return services;
	}
}
