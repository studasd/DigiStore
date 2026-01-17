using CSharpFunctionalExtensions;
using DigiStore.Framework.Endpoints;
using DigiStore.SharedKernel;
using DigiStore.WalletService.Application.Interfaces;
using DigiStore.WalletService.Contracts.Responses.Payments;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace DigiStore.WalletService.Application.Features.Payments;

/// Получить платежи пользователя
public sealed class GetUserPayments : IEndpoint
{
	//[Authorize]
	public void MapEndpoint(IEndpointRouteBuilder app)
	{
		app.MapGet("getUserPayments/{userId}", async Task<EndpointResult<IReadOnlyList<PaymentResponse>>> (
			[FromRoute] Guid userId,
			[FromServices] GetUserPaymentsHandler handler,
			CancellationToken token,
			[FromQuery] int skip = 0,
			[FromQuery] int take = 10) =>
		{
			return await handler.Handle(userId, skip, take, token);
		});
	}
}


public sealed class GetUserPaymentsHandler : IWalletServiceHandler
{
	private readonly ILogger<GetUserPaymentsHandler> _logger;
    private readonly IPaymentService _paymentService;
    private readonly IWalletRepository _walletRepository;

	public GetUserPaymentsHandler(
		ILogger<GetUserPaymentsHandler> logger,
		IPaymentService paymentService,
		IWalletRepository walletRepository)
	{
		_logger = logger;
        _paymentService = paymentService;
        _walletRepository = walletRepository;
	}



	public async Task<Result<IReadOnlyList<PaymentResponse>, Error>> Handle(Guid userId, int skip, int take, CancellationToken ct)
	{
		var paymentsResult = await _paymentService.GetUserPaymentsAsync(userId, skip, take);

		if(paymentsResult.IsFailure)
		{
			_logger.LogError("Error getting payments for user {UserId}: {Error}", userId, paymentsResult.Error);
			return paymentsResult.Error;
		}
		
		var payments = paymentsResult.Value.Select(p => new PaymentResponse(p.Id, p.Amount, p.Status, p.Description, p.CreatedAt, p.ConfirmedAt));

		return payments.ToList();
	}

}