using DigiStore.TgBot.Application.Interfaces.Repositories;
using DigiStore.TgBot.Application.Interfaces.Services;
using DigiStore.TgBot.Application.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DigiStore.TgBot.Infrastructure;

public static class DependencyInjectionExtensions
{
	public static IServiceCollection AddTgBotInfrastructure(this IServiceCollection services, IConfiguration configuration)
	{
		// Database
		var pgConnection = configuration.GetConnectionString("TgBotPostgres")
			?? throw new InvalidOperationException("TgBot PostgreSQL connection string not found");

		services.AddDbContext<Data.TgBotDbContext>(options =>
		{
			options.UseNpgsql(pgConnection);
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
