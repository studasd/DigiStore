using DigiStore.Framework.Logging;
using DigiStore.TgBot.Application;
using DigiStore.TgBot.Infrastructure;
using System.Reflection;

namespace DigiStore.TgBot.Web;


public static class DependencyInjectionExtensions
{
	public static IServiceCollection AddTgBotServices(this IServiceCollection services, IConfiguration configuration)
	{
		var seqUrl = configuration.GetValue<string>("SeqUrl");

		services
			.AddSerilogLogging("TgBot", Assembly.GetExecutingAssembly(), seqUrl)
			.AddTgBotApplication(configuration)
			.AddTgBotInfrastructure(configuration);

		return services;
	}
}
