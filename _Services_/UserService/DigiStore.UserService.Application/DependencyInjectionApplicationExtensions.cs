using DigiStore.UserService.Application.Features;
using DigiStore.UserService.Application.Features.Roles;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace DigiStore.UserService.Application;

public static class DependencyInjectionApplicationExtensions
{
	public static IServiceCollection AddCore(this IServiceCollection services, IConfiguration configuration)
	{
		services.AddValidatorsFromAssembly(typeof(DependencyInjectionApplicationExtensions).Assembly);

		services.AddScoped<ActivateUserHandler>();
		services.AddScoped<AssignRoleHandler>();
		services.AddScoped<DeactivateUserHandler>();
		services.AddScoped<GetRolesHandler>();
		services.AddScoped<GetUserByEmailHandler>();
		services.AddScoped<GetUserByIdHandler>();
		services.AddScoped<GetUserByTelegramIdHandler>();
		services.AddScoped<RegisterUserHandler>();
		services.AddScoped<RemoveRoleHandler>();
		services.AddScoped<UpdateActivityHandler>();
		services.AddScoped<UpdateLanguageHandler>();

		//services.AddStackExchangeRedisCache(setup =>
		//{
		//	setup.Configuration = "localhost:6379";
		//});

		//services.AddHybridCache(options =>
		//{
		//	options.DefaultEntryOptions = new HybridCacheEntryOptions
		//	{
		//		LocalCacheExpiration = TimeSpan.FromMinutes(5),
		//		Expiration = TimeSpan.FromMinutes(30)
		//	};
		//});

		return services;
	}
}