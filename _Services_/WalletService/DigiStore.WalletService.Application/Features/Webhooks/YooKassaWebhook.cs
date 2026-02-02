using CSharpFunctionalExtensions;
using StudCoreKit.Framework.Endpoints;
using StudCoreKit.SharedKernel;
using DigiStore.WalletService.Application.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;

namespace DigiStore.WalletService.Application.Features.Webhooks;

/// 
public sealed class YooKassaWebhook : IEndpoint
{
	//[Authorize]
	public void MapEndpoint(IEndpointRouteBuilder app)
	{
		app.MapPost("webhook/yookassa", async Task<EndpointCustomResult> (
			[FromServices] YooKassaWebhookHandler handler,
			CancellationToken token,
			HttpContext context) =>
		{
			return await handler.Handle(context, token);
		});
	}
}


public sealed class YooKassaWebhook2 : IEndpoint
{
	public void MapEndpoint(IEndpointRouteBuilder app)
	{
		app.MapPost("handle", async Task<EndpointCustomResult> (
			[FromServices] YooKassaWebhookHandler handler,
			CancellationToken token,
			HttpContext context) =>
		{
			var Request = context.Request;

			// Прочитать тело запроса
			var bodyStream = new StreamReader(Request.Body);
			var bodyContent = await bodyStream.ReadToEndAsync();

			return await handler.Handle(context, token);
		});
	}
}


public sealed class YooKassaWebhookHandler : IWalletServiceHandler
{
	private readonly ILogger<YooKassaWebhookHandler> _logger;
    private readonly IYooKassaWebhookService _yooKassaWebhookService;
    private readonly IWalletRepository _walletRepository;

	public YooKassaWebhookHandler(
		ILogger<YooKassaWebhookHandler> logger,
		IYooKassaWebhookService yooKassaWebhookService,
		IWalletRepository walletRepository)
	{
		_logger = logger;
        _yooKassaWebhookService = yooKassaWebhookService;
        _walletRepository = walletRepository;
	}



	public async Task<EndpointCustomResult> Handle(HttpContext context, CancellationToken token)
	{
		try
		{
			var Request = context.Request;

			// Прочитать тело запроса
			var bodyStream = new StreamReader(Request.Body);
			var bodyContent = await bodyStream.ReadToEndAsync();

			if (string.IsNullOrEmpty(bodyContent))
			{
				_logger.LogWarning("YooKassa: Пустое тело вебхука");
				return new EndpointCustomResult(StatusCodes.Status500InternalServerError);
				//return Error.Failure();
			}


			//// Получить подпись
			//var signature = Request.Headers["Signature"].ToString();

			//// Проверить подпись
			//if (!_yooKassaWebhookService.VerifyWebhookSignature(bodyContent, signature))
			//{
			//	_logger.LogWarning("YooKassa: Неверная подпись вебхука");
			//	return Error.Authorization();
			//}

			// Обработать вебхук
			var hookResult = await _yooKassaWebhookService.ProcessWebhookAsync(bodyContent, token);

			if(hookResult.IsFailure)
			{
				_logger.LogError("YooKassa: Ошибка при обработке вебхука - {0}", hookResult.Error.GetMessage());
				return new EndpointCustomResult(StatusCodes.Status500InternalServerError);
				//return hookResult.Error;
			}

			_logger.LogInformation("YooKassa: Вебхук успешно обработан");
			return new EndpointCustomResult(new { status = "success" });
			//return Result.Success<Error>();

			//// Вернуть 200 OK чтобы YooKassa не повторял webhook
			//return Ok(new { status = "success" });
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "YooKassa: Ошибка при обработке вебхука");

			return new EndpointCustomResult(StatusCodes.Status500InternalServerError);
			//return Error.Internal();
			//return StatusCode(500);  // YooKassa повторит позже
		}
	}

}