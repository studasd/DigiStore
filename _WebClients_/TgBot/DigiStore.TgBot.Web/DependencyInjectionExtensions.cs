using DigiStore.TgBot.Application;
using DigiStore.TgBot.Application.Options;
using DigiStore.TgBot.Infrastructure.Postgres;
using Microsoft.Extensions.Options;
using StudCoreKit.Framework.Logging;
using System.Reflection;
using StudTgBotApi.Framework;

namespace DigiStore.TgBot.Web;


public static class DependencyInjectionExtensions
{
	public static IServiceCollection AddTgBotServices(this IServiceCollection services, IConfiguration configuration)
	{
		services
			.AddTgBotApplication(configuration)
			.AddTgBotInfrastructurePostgres(configuration)
			.AddStudTgBotApi<DigiStoreApplication>(configuration)
			.AddSerilogLogging("TgBot", Assembly.GetExecutingAssembly(), sp => sp.GetRequiredService<IOptions<ServiceOptions>>().Value?.SeqUrl);

		return services;
	}
}
