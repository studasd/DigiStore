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
		app.MapGet("getUserPayments/{userId}", async Task<EndpointResult<IEnumerable<PaymentResponse>>> (
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
	private readonly IWalletRepository _walletRepository;

	public GetUserPaymentsHandler(
		ILogger<GetUserPaymentsHandler> logger,
		IWalletRepository walletRepository)
	{
		_logger = logger;
		_walletRepository = walletRepository;
	}



	public async Task<Result<IEnumerable<PaymentResponse>, Error>> Handle(Guid UserId, int skip, int take, CancellationToken ct)
	{
		var payments = await _paymentService.GetUserPaymentsAsync(userId, skip, take);

		return Ok(new
		{
			payments = payments.Select(p => new
			{
				paymentId = p.Id,
				amount = p.Amount,
				status = p.Status.ToString(),
				description = p.Description,
				createdAt = p.CreatedAt
			}).ToList(),
			total = payments.Count
		});
	}

}