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

/// Отменить выплату
public sealed class YooKassaWebhook : IEndpoint
{
	//[Authorize]
	public void MapEndpoint(IEndpointRouteBuilder app)
	{
		app.MapPost("handle", async Task<EndpointResult> (
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
	private readonly IWalletRepository _walletRepository;

	public YooKassaWebhookHandler(
		ILogger<YooKassaWebhookHandler> logger,
		IWalletRepository walletRepository)
	{
		_logger = logger;
		_walletRepository = walletRepository;
	}



	public async Task<Result<Error>> Handle(HttpContext context, CancellationToken ct)
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
				return BadRequest();
			}

			// Получить подпись
			var signature = Request.Headers["Signature"].ToString();

			// Проверить подпись
			if (!_webhookService.VerifyWebhookSignature(bodyContent, signature))
			{
				_logger.LogWarning("YooKassa: Неверная подпись вебхука");
				return Unauthorized();
			}

			// Обработать вебхук
			await _webhookService.ProcessWebhookAsync(bodyContent);

			_logger.LogInformation("YooKassa: Вебхук успешно обработан");

			return Ok();
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "YooKassa: Ошибка при обработке вебхука");
			return StatusCode(500);
		}
	}

}