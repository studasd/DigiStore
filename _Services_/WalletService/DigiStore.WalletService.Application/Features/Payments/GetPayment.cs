using CSharpFunctionalExtensions;
using StudCoreKit.Framework.Endpoints;
using StudCoreKit.SharedKernel;
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
    private readonly IPaymentService _paymentService;
    private readonly IPaymentRepository _paymentRepository;
    private readonly IWalletRepository _walletRepository;

	public GetPaymentHandler(
		ILogger<GetPaymentHandler> logger,
		IPaymentService paymentService,
		IPaymentRepository paymentRepository,
		IWalletRepository walletRepository)
	{
		_logger = logger;
        _paymentService = paymentService;
        _paymentRepository = paymentRepository;
        _walletRepository = walletRepository;
	}



	public async Task<Result<PaymentResponse, Error>> Handle(Guid paymentId, CancellationToken token)
	{
		var paymentResult = await _paymentRepository.GetByIdAsync(paymentId, token);
		if (paymentResult.IsFailure)
			return paymentResult.Error;

		var payment = paymentResult.Value;

		return new PaymentResponse(payment.Id, payment.Amount, payment.Status, payment.Description, payment.CreatedAt, payment.ConfirmedAt);
	}

}