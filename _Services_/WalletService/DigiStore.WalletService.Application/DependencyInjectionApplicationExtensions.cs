using DigiStore.SharedKernel.Extensions;
using DigiStore.WalletService.Application.Configurations;
using DigiStore.WalletService.Application.Features.Payments;
using DigiStore.WalletService.Application.Interfaces;
using DigiStore.WalletService.Application.Services;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DigiStore.WalletService.Application;

public static class DependencyInjectionApplicationExtensions
{
	public static IServiceCollection AddCore(this IServiceCollection services, IConfiguration configuration)
	{
		// Регистрируем хэндлеры
		services.AddScopedFromInterface<IWalletServiceHandler>(typeof(DependencyInjectionApplicationExtensions).Assembly);

		services.AddScoped<IWithdrawalService, WithdrawalService>();
		services.AddScoped<IPaymentService, PaymentService>();
		services.AddScoped<IPaymentRecurringService, PaymentRecurringService>();

		services.AddTransient<IValidator<CreatePaymentCommand>, CreateLessonRequestValidator>();

		services.Configure<YooKassaSettings>(configuration.GetSection(YooKassaSettings.SectionName));

		return services;
	}
}