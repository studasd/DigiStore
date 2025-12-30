using DigiStore.UserService.Application.Interfaces;
using DigiStore.UserService.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace DigiStore.UserService.Infrastructure.Postgres;

public static class DependencyInjectionExtensions
{
	public static IServiceCollection AddInfrastructurePostgres(this IServiceCollection services, IConfiguration configuration)
	{

		services.AddDbContextPool<UserDbContext>((sp, options) =>
		{
			string? connectionString = configuration.GetConnectionString(Constants.DATABASE);
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

		//services.AddDbContextPool<IReadDbContext, FileServiceDbContext>((sp, options) =>
		//{
		//	string? connectionString = configuration.GetConnectionString(Constants.DATABASE);
		//	IHostEnvironment hostEnvironment = sp.GetRequiredService<IHostEnvironment>();
		//	ILoggerFactory loggerFactory = sp.GetRequiredService<ILoggerFactory>();

		//	options.UseNpgsql(connectionString);

		//	if (hostEnvironment.IsDevelopment())
		//	{
		//		options.EnableSensitiveDataLogging();
		//		options.EnableDetailedErrors();
		//	}

		//	options.UseLoggerFactory(loggerFactory);
		//});


		// Identity
		services.AddIdentity<UserDS, RoleDS>(options =>
			{
				options.Password.RequiredLength = 8;
				options.Password.RequireDigit = true;
				options.Password.RequireNonAlphanumeric = true;
				options.Password.RequireUppercase = true;
				options.Password.RequireLowercase = true;

				options.User.RequireUniqueEmail = true;

				options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
				options.Lockout.MaxFailedAccessAttempts = 5;
			})
			.AddEntityFrameworkStores<UserDbContext>()
			.AddDefaultTokenProviders();


		//// Redis Cache
		//var redisConnection = configuration.GetConnectionString("Redis")
		//	?? throw new InvalidOperationException("Redis connection string not found");

		//var redis = ConnectionMultiplexer.Connect(redisConnection);
		//services.AddSingleton<IConnectionMultiplexer>(redis);
		//services.AddScoped<ICacheService, RedisCacheService>();

		// Repositories
		services.AddScoped<IUserRepository, UserRepository>();

		// Services


		return services;
	}
}