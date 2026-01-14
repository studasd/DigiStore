using DigiStore.WalletService.Application.Features;
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
		services.AddScoped<CheckBalanceHandler>();
		services.AddScoped<DepositHandler>();
		services.AddScoped<FreezeWalletHandler>();
		services.AddScoped<GetBalanceHandler>();
		services.AddScoped<GetTransactionsHandler>();
		services.AddScoped<GetWalletHandler>();
		services.AddScoped<PurchaseHandler>();
		services.AddScoped<RefundHandler>();
		services.AddScoped<UnfreezeWalletHandler>();
		services.AddScoped<WithdrawHandler>();

		return services;
	}
}