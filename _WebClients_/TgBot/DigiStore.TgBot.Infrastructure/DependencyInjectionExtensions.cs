using DigiStore.TgBot.Application.Interfaces.Repositories;
using DigiStore.TgBot.Application.Interfaces.Services;
using DigiStore.TgBot.Application.Services;
using DigiStore.TgBot.Infrastructure.Data;
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
		// Database
		services.AddDbContextPool<TgBotDbContext>((sp, options) =>
		{
			string? connectionString = configuration.GetConnectionString("TgBotPostgres");
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


		// Session & Localization
		services.AddScoped<ITelegramSessionService, TelegramSessionService>();
		services.AddScoped<ILocalizationService, LocalizationService>();

		// Repositories
		services.AddScoped<ITelegramUserRepository, Repositories.UserRepository>();
		services.AddScoped<ITelegramSessionRepository, Repositories.SessionRepository>();
		services.AddScoped<ICommandHistoryRepository, Repositories.CommandHistoryRepository>();
		services.AddScoped<ILocalizationRepository, Repositories.LocalizationRepository>();

		//// Redis
		//var redisConnection = configuration.GetConnectionString("Redis")
		//	?? throw new InvalidOperationException("Redis connection string not found");
		//var redis = ConnectionMultiplexer.Connect(redisConnection);
		//services.AddSingleton<IConnectionMultiplexer>(redis);

		return services;
	}
}
