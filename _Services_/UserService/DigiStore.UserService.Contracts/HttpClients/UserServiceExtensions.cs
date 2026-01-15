using DigiStore.SharedKernel.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace DigiStore.UserService.Contracts.HttpClients;


public static class UserServiceExtensions
{
	public static IServiceCollection AddUserServiceHttp(this IServiceCollection services, IConfiguration configuration)
	{
		services.Configure<UserServiceOptions>(configuration.GetSection(nameof(UserServiceOptions)));

		services.AddScoped<IUserHttpClient, UserHttpClient>();


		services.AddHttpServiceFactory()
			.AddHttpService<UserHttpClient>((sp, opts) =>
			{
				var fileOptions = sp.GetRequiredService<IOptions<UserServiceOptions>>().Value;

				opts.Url = fileOptions.Url;
				opts.TimeoutSeconds = fileOptions.TimeoutSeconds;
			});


		return services;
	}
}