using DigiStore.WalletService.Application.Interfaces;
using DigiStore.WalletService.Infrastructure.Postgres.Data;
using DigiStore.WalletService.Infrastructure.Postgres.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace DigiStore.WalletService.Infrastructure.Postgres;

public static class DependencyInjectionExtensions
{
	public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
	{

		services.AddDbContextPool<WalletDbContext>((sp, options) =>
		{
			string? connectionString = configuration.GetConnectionString("Database");
			IHostEnvironment hostEnvironment = sp.GetRequiredService<IHostEnvironment>();
			ILoggerFactory loggerFactory = sp.GetRequiredService<ILoggerFactory>();

			options.UseNpgsql(connectionString);

			if (hostEnvironment.IsDevelopment())
			{
				options.EnableSensitiveDataLogging();
				options.EnableDetailedErrors();
			}

			options.UseLoggerFactory(loggerFactory);
		});


		// Repositories
		services.AddScoped<IWalletRepository, WalletRepository>();

		// Services


		return services;
	}
}