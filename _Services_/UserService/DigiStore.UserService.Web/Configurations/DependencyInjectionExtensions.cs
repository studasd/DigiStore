using DigiStore.Framework.Endpoints;
using DigiStore.Framework.Logging;
using DigiStore.Framework.Swagger;
using DigiStore.UserService.Application;
using DigiStore.UserService.Infrastructure.Postgres;
using Microsoft.AspNetCore.Routing.Constraints;
using System.Reflection;

namespace DigiStore.UserService.Web.Configurations;

public static class DependencyInjectionExtensions
{
	public static IServiceCollection AddConfiguration(this IServiceCollection services, IConfiguration configuration)
	{
		var seqUrl = configuration.GetValue<string>("SeqUrl");
		//.AddSerilogLogging(
		//		"TgBot",
		//		Assembly.GetExecutingAssembly(),
		//		sp => sp.GetRequiredService<IOptions<ServiceOptions>>().Value?.SeqUrl);

		services
			.AddCore(configuration)
			.AddInfrastructurePostgres(configuration)
			//.AddSerilogLogging(configuration, "UserService")
			.AddSerilogLogging("UserService", Assembly.GetExecutingAssembly(), seqUrl)
			.AddOpenApiSpec("UserService", "v1")
			.AddEndpoints(typeof(DependencyInjectionApplicationExtensions).Assembly)
			;

		return services;
	}
}