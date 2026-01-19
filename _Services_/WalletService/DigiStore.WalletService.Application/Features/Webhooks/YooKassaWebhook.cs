using CSharpFunctionalExtensions;
using DigiStore.Framework.Endpoints;
using DigiStore.SharedKernel;
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
		app.MapPost("webhook/yookassa", async Task<EndpointResult> (
			[FromServices] YooKassaWebhookHandler handler,
			CancellationToken token,
			HttpContext context) =>
		{
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



	public async Task<UnitResult<Error>> Handle(HttpContext context, CancellationToken token)
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
				return Error.Failure();
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
			await _yooKassaWebhookService.ProcessWebhookAsync(bodyContent, token);

			_logger.LogInformation("YooKassa: Вебхук успешно обработан");

			return Result.Success<Error>();
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "YooKassa: Ошибка при обработке вебхука");
			return Error.Internal();
		}
	}

}