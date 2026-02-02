using DigiStore.WalletService.Application.Configurations;
using DigiStore.WalletService.Application.Interfaces;
using DigiStore.WalletService.Application.Services;
using DigiStore.WalletService.Application.Validators;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StudCoreKit.Framework.Endpoints;
using StudCoreKit.SharedKernel.Extensions;

namespace DigiStore.WalletService.Application;

public static class DependencyInjectionApplicationExtensions
{
	public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
	{
		// Регистрируем все эндпоинты из текущей сборки
		services.AddEndpoints(typeof(DependencyInjectionApplicationExtensions).Assembly);
		// Регистрируем хэндлеры
		services.AddScopedFromInterface<IWalletServiceHandler>(typeof(DependencyInjectionApplicationExtensions).Assembly);

		services.AddValidatorsFromAssembly(typeof(DependencyInjectionApplicationExtensions).Assembly);


		services.AddScoped<IWithdrawalService, WithdrawalService>();
		services.AddScoped<IPaymentService, PaymentService>();
		services.AddScoped<IPaymentRecurringService, PaymentRecurringService>();

		// Регистрировать валидаторы
		services.AddSingleton<PaymentValidator>();
		services.AddSingleton<WithdrawalValidator>();

		services.Configure<YooKassaSettings>(configuration.GetSection(YooKassaSettings.SectionName));

		return services;
	}
}