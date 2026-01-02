using DigiStore.UserService.Web.Configurations;
using Microsoft.AspNetCore.Routing.Constraints;
using Serilog;
using System.Globalization;

Log.Logger = new LoggerConfiguration()
	.MinimumLevel.Information()
	.WriteTo.Console(formatProvider: CultureInfo.InvariantCulture)
	.CreateBootstrapLogger();

try
{
	Log.Information("Starting web application");

	var builder = WebApplication.CreateBuilder(args);

	// ✅ РЕГИСТРАЦИЯ ВСЕХ НУЖНЫХ CONSTRAINTS
	builder.Services.Configure<RouteOptions>(options =>
	{
		// Regex и другие
		options.SetParameterPolicy<RegexInlineRouteConstraint>("regex");
	});

	builder.Services.AddEndpointsApiExplorer();

	// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
	builder.Services.AddOpenApi();


	builder.Services.AddConfiguration(builder.Configuration);

	builder.Services.AddCors();


	var app = builder.Build();

	app.Configure();

	app.Run();
}
catch (Exception ex)
{
	Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
	Log.CloseAndFlush();
}

