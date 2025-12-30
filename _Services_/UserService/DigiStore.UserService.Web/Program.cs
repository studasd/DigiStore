using DigiStore.UserService.Infrastructure.Postgres;
using DigiStore.UserService.Web;
using DigiStore.UserService.Web.Configurations;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System.Globalization;
using System.Text.Json.Serialization;

Log.Logger = new LoggerConfiguration()
	.MinimumLevel.Information()
	.WriteTo.Console(formatProvider: CultureInfo.InvariantCulture)
	.CreateBootstrapLogger();

try
{
	Log.Information("Starting web application");

	var builder = WebApplication.CreateSlimBuilder(args);


	// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
	builder.Services.AddOpenApi();


	builder.Services.AddConfiguration(builder.Configuration);

	builder.Services.AddCors();



	var app = builder.Build();

	app.Configure();

}
catch (Exception ex)
{
	Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
	Log.CloseAndFlush();
}

