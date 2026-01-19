using DigiStore.WalletService.Application;
using DigiStore.WalletService.Application.Configurations;
using DigiStore.WalletService.Application.Interfaces;
using DigiStore.WalletService.Infrastructure.Yookassa.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Yandex.Checkout.V3;

namespace DigiStore.WalletService.Infrastructure.Yookassa;

public static class DependencyInjectionExtensions
{
	public static IServiceCollection AddInfrastructureYookassa(this IServiceCollection services, IConfiguration configuration)
	{
		services.AddHostedService<RecurringPaymentBackgroundService>();

		
		// Регистрировать YooKassa Client
		services.AddSingleton<Client>(provider =>
		{
			var opts = provider.GetRequiredService<IOptions<YooKassaSettings>>().Value;

			// Валидировать параметры
			if (string.IsNullOrEmpty(opts.ShopId))
				throw new InvalidOperationException("YooKassa ShopId не установлен");

			if (string.IsNullOrEmpty(opts.SecretKey))
				throw new InvalidOperationException("YooKassa SecretKey не установлен");

			return new Client(
				shopId: opts.ShopId,
				secretKey: opts.SecretKey);
		});

		// Регистрировать сервисы
		services.AddScoped<IYookassaProvider, YookassaProvider>();
		//services.AddScoped<IYooKassaWebhookService, YooKassaWebhookService>();
		services.AddScoped<IYooKassaWebhookService, YooKassaWebhookService>();


		return services;
	}
}