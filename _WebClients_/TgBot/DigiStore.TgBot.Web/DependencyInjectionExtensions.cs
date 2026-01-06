using DigiStore.TgBot.Application;
using DigiStore.TgBot.Infrastructure;

namespace DigiStore.TgBot.Web;


public static class DependencyInjectionExtensions
{
	public static IServiceCollection AddTgBotServices(this IServiceCollection services, IConfiguration configuration)
	{
		services
			.AddTgBotApplication(configuration)
			.AddTgBotInfrastructure(configuration);

		return services;
	}
}
