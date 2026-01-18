using DigiStore.WalletService.Web.Configurations;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Constraints;
using System.Text.Json.Serialization;


var builder = WebApplication.CreateSlimBuilder(args);

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

// Добавить контроллеры
builder.Services.AddControllers()
	.AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

// apply same converter for minimal API (input binding / output for MapPost etc.)
builder.Services.ConfigureHttpJsonOptions(opts =>
{
	opts.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
	app.MapOpenApi();
}

app.Configure();

app.Run();
