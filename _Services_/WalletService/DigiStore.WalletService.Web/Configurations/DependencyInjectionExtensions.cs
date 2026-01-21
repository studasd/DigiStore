using DigiStore.Framework.Endpoints;
using DigiStore.Framework.Logging;
using DigiStore.Framework.Swagger;
using DigiStore.WalletService.Application;
using DigiStore.WalletService.Infrastructure.Postgres;
using DigiStore.WalletService.Infrastructure.Yookassa;
using System.Reflection;
using DigiStore.TgBot.Contracts.HttpClients;

namespace DigiStore.WalletService.Web.Configurations;

public static class DependencyInjectionExtensions
{
	public static IServiceCollection AddConfiguration(this IServiceCollection services, IConfiguration configuration)
	{
		var seqUrl = configuration.GetValue<string>("SeqUrl");

		// Добавить Background Service
		services.AddHostedService<RecurringPaymentBackgroundService>();

		services
			.AddApplication(configuration)
			.AddInfrastructurePostgres(configuration)
			.AddInfrastructureYookassa(configuration)
			.AddTgBotHttp(configuration)
			//.AddSerilogLogging(configuration, "WalletService")
			.AddSerilogLogging("WalletService", Assembly.GetExecutingAssembly(), seqUrl)
			.AddOpenApiSpec("WalletService", "v1")
			;

		return services;
	}
}