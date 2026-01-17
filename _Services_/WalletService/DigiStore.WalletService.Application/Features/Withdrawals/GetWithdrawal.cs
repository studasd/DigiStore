using CSharpFunctionalExtensions;
using DigiStore.Framework.Endpoints;
using DigiStore.SharedKernel;
using DigiStore.WalletService.Application.Interfaces;
using DigiStore.WalletService.Contracts.Requests.Withdrawals;
using DigiStore.WalletService.Contracts.Responses.Withdrawals;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace DigiStore.WalletService.Application.Features.Withdrawals;


/// Получить информацию о выплате
public sealed class GetWithdrawal : IEndpoint
{
	//[Authorize]
	public void MapEndpoint(IEndpointRouteBuilder app)
	{
		app.MapGet("getWithdrawal/{withdrawalId}", async Task<EndpointResult<WithdrawalResponse>> (
			[FromRoute] Guid withdrawalId,
			[FromServices] GetWithdrawalHandler handler,
			CancellationToken token) =>
		{
			return await handler.Handle(withdrawalId, token);
		});
	}
}


public sealed class GetWithdrawalHandler : IWalletServiceHandler
{
	private readonly ILogger<GetWithdrawalHandler> _logger;
    private readonly IWithdrawalRepository _withdrawalRepository;
    private readonly IWalletRepository _walletRepository;

	public GetWithdrawalHandler(
		ILogger<GetWithdrawalHandler> logger,
		IWithdrawalRepository withdrawalRepository,
		IWalletRepository walletRepository)
	{
		_logger = logger;
        _withdrawalRepository = withdrawalRepository;
        _walletRepository = walletRepository;
	}



	public async Task<Result<WithdrawalResponse, Error>> Handle(Guid withdrawalId, CancellationToken ct)
	{
		var withdrawalResult = await _withdrawalRepository.GetByIdAsync(withdrawalId, ct);
		if (withdrawalResult.IsFailure)
			return withdrawalResult.Error;

		var withdrawal = withdrawalResult.Value;

		return new WithdrawalResponse
		(
			withdrawal!.Id,
			withdrawal.RequestedAmount,
			withdrawal.Commission,
			withdrawal.ActualAmount,
			withdrawal.CardMask,
			withdrawal.Status,
			withdrawal.CreatedAt,
			withdrawal.CompletedAt
		);
	}

}