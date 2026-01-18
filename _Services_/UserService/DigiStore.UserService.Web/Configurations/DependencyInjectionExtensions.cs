using DigiStore.Framework.Logging;
using DigiStore.Framework.Swagger;
using DigiStore.UserService.Application;
using DigiStore.UserService.Infrastructure.Postgres;
using System.Reflection;

namespace DigiStore.UserService.Web.Configurations;

public static class DependencyInjectionExtensions
{
	public static IServiceCollection AddConfiguration(this IServiceCollection services, IConfiguration configuration)
	{
		var seqUrl = configuration.GetValue<string>("SeqUrl");

		services
			.AddApplication(configuration)
			.AddInfrastructurePostgres(configuration)
			//.AddSerilogLogging(configuration, "UserService")
			.AddSerilogLogging("UserService", Assembly.GetExecutingAssembly(), seqUrl)
			.AddOpenApiSpec("UserService", "v1")
			;

		return services;
	}
}