using CSharpFunctionalExtensions;
using DigiStore.Framework.Endpoints;
using DigiStore.SharedKernel;
using DigiStore.WalletService.Application.Interfaces;
using DigiStore.WalletService.Domain.Enums;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;

namespace DigiStore.WalletService.Application.Features.Withdrawals;


/// Отменить выплату
public sealed class CancelWithdrawal : IEndpoint
{
	//[Authorize]
	public void MapEndpoint(IEndpointRouteBuilder app)
	{
		app.MapPost("cancel/{withdrawalId}", async Task<EndpointResult<string>> (
			[FromRoute] Guid withdrawalId,
			[FromServices] CancelWithdrawalHandler handler,
			CancellationToken token) =>
		{
			return await handler.Handle(withdrawalId, token);
		});
	}
}


public sealed class CancelWithdrawalHandler : IWalletServiceHandler
{
	private readonly ILogger<CancelWithdrawalHandler> _logger;
    private readonly IWithdrawalService _withdrawalService;
    private readonly IWithdrawalRepository _withdrawalRepository;
    private readonly IWalletRepository _walletRepository;

	public CancelWithdrawalHandler(
		ILogger<CancelWithdrawalHandler> logger,
		IWithdrawalService _withdrawalService,
		IWithdrawalRepository withdrawalRepository,
		IWalletRepository walletRepository)
	{
		_logger = logger;
        this._withdrawalService = _withdrawalService;
        _withdrawalRepository = withdrawalRepository;
        _walletRepository = walletRepository;
	}



	public async Task<Result<string, Error>> Handle(Guid withdrawalId, CancellationToken ct)
	{
		var withdrawalResult = await _withdrawalRepository.GetByIdAsync(withdrawalId, ct);

		if(withdrawalResult.IsFailure)
			return withdrawalResult.Error;

		var withdrawal = withdrawalResult.Value;
		if (withdrawal.Status != WithdrawalStatus.Pending && withdrawal.Status != WithdrawalStatus.Processing)
			return Error.Conflict("cancel.bad", "Выплату нельзя отменить в этом статусе");

		await _withdrawalService.CancelWithdrawalAsync(withdrawalId, "Отменено пользователем", ct);

		return "Выплата отменена";
	}

}