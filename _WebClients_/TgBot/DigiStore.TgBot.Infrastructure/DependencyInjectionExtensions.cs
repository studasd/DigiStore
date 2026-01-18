using DigiStore.TgBot.Application.Interfaces.Repositories;
using DigiStore.TgBot.Application.Interfaces.Services;
using DigiStore.TgBot.Application.Services;
using DigiStore.TgBot.Infrastructure.Data;
using DigiStore.TgBot.Infrastructure.Data.Seeders;
using DigiStore.UserService.Contracts.HttpClients;
using DigiStore.WalletService.Contracts.HttpClients;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DigiStore.TgBot.Infrastructure;

public static class DependencyInjectionExtensions
{
	public static IServiceCollection AddTgBotInfrastructure(this IServiceCollection services, IConfiguration configuration)
	{
		// Repositories
		services.AddScoped<ITgUserRepository, Repositories.TgUserRepository>();
		services.AddScoped<ISessionRepository, Repositories.SessionRepository>();
		services.AddScoped<ICommandHistoryRepository, Repositories.CommandHistoryRepository>();
		services.AddScoped<ILocalizationRepository, Repositories.LocalizationRepository>();
		
		services.AddScoped<IDataSeeder, DataSeeder>();

		services.AddScoped<IWalletService, Services.WalletService>();
		services.AddScoped<ITgUserService, Services.TgUserService>();

		services.AddWalletServiceHttp(configuration);
		services.AddUserServiceHttp(configuration);

		// Database
		services.AddDbContextPool<TgBotDbContext>((sp, options) =>
		{
			string? connectionString = configuration.GetConnectionString("TgBotPostgres");
			IHostEnvironment hostEnvironment = sp.GetRequiredService<IHostEnvironment>();
			ILoggerFactory loggerFactory = sp.GetRequiredService<ILoggerFactory>();

			options.UseNpgsql(connectionString)
				//.UseAsyncSeeding(async (context, result, token) =>
				//{
				//	var seeder = ActivatorUtilities.CreateInstance<DataSeeder>(sp);
				//	await seeder.SeedAsync((TgBotDbContext)context, sp, CancellationToken.None);
				//});
				;

			if (hostEnvironment.IsDevelopment())
			{
				options.EnableSensitiveDataLogging();
				options.EnableDetailedErrors();
			}

			options.UseLoggerFactory(loggerFactory);
		});


		//// Redis
		//var redisConnection = configuration.GetConnectionString("Redis")
		//	?? throw new InvalidOperationException("Redis connection string not found");
		//var redis = ConnectionMultiplexer.Connect(redisConnection);
		//services.AddSingleton<IConnectionMultiplexer>(redis);

		return services;
	}
}
