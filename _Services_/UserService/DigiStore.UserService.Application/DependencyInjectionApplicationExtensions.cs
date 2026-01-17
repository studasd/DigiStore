using DigiStore.Framework.Endpoints;
using DigiStore.SharedKernel.Extensions;
using DigiStore.UserService.Application.Interfaces;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DigiStore.UserService.Application;

public static class DependencyInjectionApplicationExtensions
{
	public static IServiceCollection AddCore(this IServiceCollection services, IConfiguration configuration)
	{
		services.AddValidatorsFromAssembly(typeof(DependencyInjectionApplicationExtensions).Assembly);

		// Регистрируем все эндпоинты из текущей сборки
		services.AddEndpoints(typeof(DependencyInjectionApplicationExtensions).Assembly);

		// Automatically register all handlers that implement IUserServiceHandler
		services.AddScopedFromInterface<IUserServiceHandler>(typeof(DependencyInjectionApplicationExtensions).Assembly);



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