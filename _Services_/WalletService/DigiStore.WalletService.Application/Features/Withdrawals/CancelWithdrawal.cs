using CSharpFunctionalExtensions;
using DigiStore.Framework.Endpoints;
using DigiStore.SharedKernel;
using DigiStore.WalletService.Application.Interfaces;
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
		app.MapPost("cancel/{withdrawalId}", async Task<EndpointResult<CancelWithdrawalResponse>> (
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
	private readonly IWalletRepository _walletRepository;

	public CancelWithdrawalHandler(
		ILogger<CancelWithdrawalHandler> logger,
		IWalletRepository walletRepository)
	{
		_logger = logger;
		_walletRepository = walletRepository;
	}



	public async Task<Result<CancelWithdrawalResponse, Error>> Handle(Guid withdrawalId, CancellationToken ct)
	{
		var withdrawal = await _withdrawalService.GetWithdrawalAsync(withdrawalId);
		if (withdrawal == null)
			return NotFound(new { error = "Выплата не найдена" });

		if (withdrawal.Status.ToString() != "Pending" && withdrawal.Status.ToString() != "Processing")
			return BadRequest(new { error = "Выплату нельзя отменить в этом статусе" });

		await _withdrawalService.CancelWithdrawalAsync(withdrawalId, "Отменено пользователем");

		return Ok(new { message = "Выплата отменена" });
	}

}