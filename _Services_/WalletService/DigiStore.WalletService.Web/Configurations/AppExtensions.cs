using DigiStore.Framework.Endpoints;
using DigiStore.Framework.Middlewares;
using Serilog;

namespace DigiStore.WalletService.Web.Configurations;

public static class AppExtensions
{
	public static IApplicationBuilder Configure(this WebApplication app)
	{
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