using DigiStore.SharedKernel.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace DigiStore.TgBot.Contracts.HttpClients;


public static class TgBotExtensions
{
	public static IServiceCollection AddTgBotHttp(this IServiceCollection services, IConfiguration configuration)
	{
		services.Configure<TgBotOptions>(configuration.GetSection(nameof(TgBotOptions)));

		services.AddScoped<ITgBotHttpClient, TgBotHttpClient>();


		services.AddHttpServiceFactory()
			.AddHttpService<TgBotHttpClient>((sp, opts) =>
			{
				var fileOptions = sp.GetRequiredService<IOptions<TgBotOptions>>().Value;

				opts.Url = fileOptions.Url;
				opts.TimeoutSeconds = fileOptions.TimeoutSeconds;
			});


		return services;
	}
}