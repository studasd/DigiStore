using DigiStore.SharedKernel.Extensions;
using DigiStore.WalletService.Application.Features;
using DigiStore.WalletService.Application.Interfaces;
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
		services.AddScopedFromInterface<IUserServiceHandler>(typeof(DependencyInjectionApplicationExtensions).Assembly);

		return services;
	}
}