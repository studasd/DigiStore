using StudCoreKit.SharedKernel.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace DigiStore.WalletService.Contracts.HttpClients;


public static class WalletServiceExtensions
{
	public static IServiceCollection AddWalletServiceHttp(this IServiceCollection services, IConfiguration configuration)
	{
		services.Configure<WalletServiceOptions>(configuration.GetSection(nameof(WalletServiceOptions)));

		services.AddScoped<IWalletHttpClient, WalletHttpClient>();


		services.AddHttpServiceFactory()
			.AddHttpService<WalletHttpClient>((sp, opts) =>
			{
				var fileOptions = sp.GetRequiredService<IOptions<WalletServiceOptions>>().Value;

				opts.Url = fileOptions.Url;
				opts.TimeoutSeconds = fileOptions.TimeoutSeconds;
			});


		return services;
	}
}