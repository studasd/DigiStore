using DigiStore.Framework.Logging;
using DigiStore.TgBot.Application;
using DigiStore.TgBot.Infrastructure;
using DigiStore.TgBot.Infrastructure.BotAPI;
using System.Reflection;
using DigiStore.TgBot.Application.Options;
using Microsoft.Extensions.Options;

namespace DigiStore.TgBot.Web;


public static class DependencyInjectionExtensions
{
	public static IServiceCollection AddTgBotServices(this IServiceCollection services, IConfiguration configuration)
	{
		services
			.AddTgBotApplication(configuration)
			.AddTgBotInfrastructure(configuration)
			.AddTgBotAPIInfrastructure(configuration)
			.AddSerilogLogging("TgBot", Assembly.GetExecutingAssembly(), sp => sp.GetRequiredService<IOptions<ServiceOptions>>().Value?.SeqUrl);

		return services;
	}
}
