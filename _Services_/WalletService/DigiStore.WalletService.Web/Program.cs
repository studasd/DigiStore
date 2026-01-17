using DigiStore.WalletService.Web.Configurations;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Constraints;


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
builder.Services.AddControllers();


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
	app.MapOpenApi();
}

app.Configure();

app.Run();
