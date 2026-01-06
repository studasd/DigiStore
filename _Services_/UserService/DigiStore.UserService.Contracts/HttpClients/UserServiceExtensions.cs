using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace DigiStore.UserService.Contracts.HttpClients;


public static class UserServiceExtensions
{
	public static IServiceCollection AddUserServiceHttp(this IServiceCollection services, IConfiguration configuration)
	{
		services.Configure<UserServiceOptions>(configuration.GetSection(nameof(UserServiceOptions)));

		services.AddHttpClient<IUserHttpClient, UserHttpClient>((sp, config) =>
		{
			UserServiceOptions fileOptions = sp.GetRequiredService<IOptions<UserServiceOptions>>().Value;

			config.BaseAddress = new Uri(fileOptions.Url);

			config.Timeout = TimeSpan.FromSeconds(fileOptions.TimeoutSeconds);
		});

		return services;
	}
}