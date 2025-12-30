using DigiStore.Framework.Endpoints;
using DigiStore.Framework.Middlewares;
using Serilog;

namespace DigiStore.UserService.Web.Configurations;

public static class AppExtensions
{
	public static IApplicationBuilder Configure(this WebApplication app)
	{
		//app.UseCors(builder =>
		//{
		//	builder.WithOrigins(
		//			"http://localhost:3000",
		//			"http://localhost:3001",
		//			"http://localhost",
		//			"http://frontend:3000")
		//		.AllowCredentials()
		//		.AllowAnyHeader()
		//		.AllowAnyMethod();
		//});

		app.UseExceptionMiddleware();
		app.UseRequestCorrelationId();
		app.UseSerilogRequestLogging();

		app.MapOpenApi();

		app.UseSwagger();
		app.UseSwaggerUI(options =>
		{
			options.SwaggerEndpoint("/openapi/v1.json", "MediaAsset Service V1");
		});

		app.MapEndpoints();

		return app;
	}
}