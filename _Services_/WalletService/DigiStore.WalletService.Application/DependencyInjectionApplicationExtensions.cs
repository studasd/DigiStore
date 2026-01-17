using DigiStore.SharedKernel.Extensions;
using DigiStore.WalletService.Application.Configurations;
using DigiStore.WalletService.Application.Features;
using DigiStore.WalletService.Application.Features.Payments;
using DigiStore.WalletService.Application.Interfaces;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace DigiStore.WalletService.Application;

public static class DependencyInjectionApplicationExtensions
{
	public static IServiceCollection AddCore(this IServiceCollection services, IConfiguration configuration)
	{
		// Регистрируем хэндлеры
		services.AddScopedFromInterface<IWalletServiceHandler>(typeof(DependencyInjectionApplicationExtensions).Assembly);

		services.Configure<YooKassaSettings>(configuration.GetSection("YooKassaSettings"));
		services.AddTransient<IValidator<CreatePaymentCommand>, CreateLessonRequestValidator>();

		return services;
	}
}