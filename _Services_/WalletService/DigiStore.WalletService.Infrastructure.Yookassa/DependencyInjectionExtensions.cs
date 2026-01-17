using DigiStore.WalletService.Application;
using DigiStore.WalletService.Application.Configurations;
using DigiStore.WalletService.Application.Interfaces;
using DigiStore.WalletService.Application.Validators;
using DigiStore.WalletService.Infrastructure.Yookassa.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Yandex.Checkout.V3;

namespace DigiStore.WalletService.Infrastructure.Yookassa;

public static class DependencyInjectionExtensions
{
	public static IServiceCollection AddInfrastructureYookassa(this IServiceCollection services, IConfiguration configuration)
	{
		services.AddHostedService<RecurringPaymentBackgroundService>();

		// Загрузить конфигурацию
		var yooKassaSettings = new YooKassaSettings();
		configuration.GetSection(YooKassaSettings.SectionName).Bind(yooKassaSettings);

		// Валидировать параметры
		if (string.IsNullOrEmpty(yooKassaSettings.ShopId))
			throw new InvalidOperationException("YooKassa ShopId не установлен");

		if (string.IsNullOrEmpty(yooKassaSettings.SecretKey))
			throw new InvalidOperationException("YooKassa SecretKey не установлен");

		
		// Регистрировать YooKassa Client
		services.AddSingleton<Client>(provider =>
		{

			return new Client(
				shopId: yooKassaSettings.ShopId,
				secretKey: yooKassaSettings.SecretKey);
		});

		// Регистрировать сервисы
		services.AddScoped<IYookassaProvider, YookassaProvider>();
		services.AddScoped<IYooKassaWebhookService, YooKassaWebhookService>();

		// Регистрировать валидаторы
		services.AddSingleton<PaymentValidator>();
		services.AddSingleton<WithdrawalValidator>();

		return services;
	}
}