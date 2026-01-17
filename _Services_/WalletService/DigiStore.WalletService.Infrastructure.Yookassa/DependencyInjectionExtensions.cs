using DigiStore.WalletService.Application;
using DigiStore.WalletService.Application.Configurations;
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

		services.AddYooKassaInfrastructure(configuration);
		services.AddHostedService<RecurringPaymentBackgroundService>();



		// Загрузить конфигурацию
		var yooKassaSettings = new YooKassaSettings();
		configuration.GetSection(YooKassaSettings.SectionName).Bind(yooKassaSettings);

		services.AddSingleton(yooKassaSettings);
		services.AddSingleton<PaymentValidator>();
		services.AddSingleton<WithdrawalValidator>();

		// Регистрировать клиент YooKassa версии 4.3.1
		// ВАЖНО: Используем Client из библиотеки, а не кастомный
		services.AddScoped<Client>(provider =>
		{
			return new Client(
				shopId: yooKassaSettings.ShopId,
				secretKey: yooKassaSettings.SecretKey);
		});

		// Регистрировать сервисы
		services.AddScoped<YookassaProvider>();
		//services.AddScoped<YooKassaWithdrawalService>();
		services.AddScoped<YooKassaWebhookService>();
		//services.AddScoped<YooKassaRecurringService>();

		return services;
	}



	/// <summary>
	/// Добавить YooKassa инфраструктуру
	/// </summary>
	public static IServiceCollection AddYooKassaInfrastructure(
		this IServiceCollection services,
		IConfiguration configuration)
	{
		// Загрузить параметры из конфига
		var yooKassaSettings = new YooKassaSettings();
		configuration.GetSection(YooKassaSettings.SectionName).Bind(yooKassaSettings);

		// Валидировать параметры
		if (string.IsNullOrEmpty(yooKassaSettings.ShopId))
			throw new InvalidOperationException("YooKassa ShopId не установлен");

		if (string.IsNullOrEmpty(yooKassaSettings.SecretKey))
			throw new InvalidOperationException("YooKassa SecretKey не установлен");

		// Регистрировать параметры как Singleton
		services.AddSingleton(yooKassaSettings);

		// Регистрировать валидаторы
		services.AddSingleton<PaymentValidator>();
		services.AddSingleton<WithdrawalValidator>();

		// Регистрировать YooKassa Client
		services.AddSingleton<Client>(provider =>
		{
			return new Client(
				shopId: yooKassaSettings.ShopId,
				secretKey: yooKassaSettings.SecretKey);
		});

		// Регистрировать сервисы
		services.AddScoped<YookassaProvider>();
		//services.AddScoped<YooKassaWithdrawalService>();
		//services.AddScoped<YooKassaRecurringService>();
		services.AddScoped<YooKassaWebhookService>();

		return services;
	}
}