using DigiStore.Framework.Endpoints;
using DigiStore.Framework.Logging;
using DigiStore.Framework.Swagger;
using DigiStore.WalletService.Application;
using DigiStore.WalletService.Infrastructure.Postgres;
using System.Reflection;

namespace DigiStore.WalletService.Web.Configurations;

public static class DependencyInjectionExtensions
{
	public static IServiceCollection AddConfiguration(this IServiceCollection services, IConfiguration configuration)
	{
		var seqUrl = configuration.GetValue<string>("SeqUrl");

		services
			.AddCore(configuration)
			.AddInfrastructure(configuration)
			//.AddSerilogLogging(configuration, "WalletService")
			.AddSerilogLogging("WalletService", Assembly.GetExecutingAssembly(), seqUrl)
			.AddOpenApiSpec("WalletService", "v1")
			.AddEndpoints(typeof(DependencyInjectionApplicationExtensions).Assembly)
			;

		return services;
	}
}