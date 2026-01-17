using CSharpFunctionalExtensions;
using DigiStore.Framework.Endpoints;
using DigiStore.SharedKernel;
using DigiStore.WalletService.Application.Interfaces;
using DigiStore.WalletService.Contracts.Responses.Payments;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;

namespace DigiStore.WalletService.Application.Features.Payments;


/// Получить информацию о платеже
public sealed class GetPayment : IEndpoint
{
	//[Authorize]
	public void MapEndpoint(IEndpointRouteBuilder app)
	{
		app.MapGet("getPayment/{paymentId}", async Task<EndpointResult<PaymentResponse>> (
			[FromRoute] Guid paymentId,
			[FromServices] GetPaymentHandler handler,
			CancellationToken token) =>
		{
			return await handler.Handle(paymentId, token);
		});
	}
}


public sealed class GetPaymentHandler : IWalletServiceHandler
{
	private readonly ILogger<GetPaymentHandler> _logger;
	private readonly IWalletRepository _walletRepository;

	public GetPaymentHandler(
		ILogger<GetPaymentHandler> logger,
		IWalletRepository walletRepository)
	{
		_logger = logger;
		_walletRepository = walletRepository;
	}



	public async Task<Result<PaymentResponse, Error>> Handle(Guid paymentId, CancellationToken ct)
	{
		var payment = await _paymentService.GetPaymentAsync(paymentId);
		if (payment == null)
			return NotFound(new { error = "Платеж не найден" });

		return Ok(new
		{
			paymentId = payment.Id,
			amount = payment.Amount,
			status = payment.Status.ToString(),
			description = payment.Description,
			createdAt = payment.CreatedAt,
			confirmedAt = payment.ConfirmedAt
		});
	}

}