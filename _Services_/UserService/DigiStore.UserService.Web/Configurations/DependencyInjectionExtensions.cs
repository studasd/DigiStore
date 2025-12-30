using DigiStore.Framework.Endpoints;
using DigiStore.Framework.Logging;
using DigiStore.Framework.Swagger;
using DigiStore.UserService.Application;
using DigiStore.UserService.Infrastructure.Postgres;

namespace DigiStore.UserService.Web.Configurations;

public static class DependencyInjectionExtensions
{
	public static IServiceCollection AddConfiguration(this IServiceCollection services, IConfiguration configuration)
	{
		services
			.AddSerilogLogging(configuration, "UserService")
			.AddOpenApiSpec("UserService", "v1")
			.AddEndpoints(typeof(DependencyInjectionApplicationExtensions).Assembly)
			;

		services
			.AddCore(configuration)
			.AddInfrastructurePostgres(configuration);

		return services;
	}
}