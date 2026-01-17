using CSharpFunctionalExtensions;
using DigiStore.Framework.Endpoints;
using DigiStore.WalletService.Application.Interfaces;
using DigiStore.WalletService.Contracts.Responses.Payments;
using DigiStore.WalletService.Contracts.Responses.Withdrawals;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;

namespace DigiStore.WalletService.Application.Features.Withdrawals;

/// Получить выплаты пользователя
public sealed class GetUserWithdrawals : IEndpoint
{
	//[Authorize]
	public void MapEndpoint(IEndpointRouteBuilder app)
	{
		app.MapGet("getUserWithdrawals/{userId}", async Task<EndpointResult<IEnumerable<WithdrawalResponse>>> (
			[FromRoute] Guid userId,
			[FromServices] GetUserWithdrawalsHandler handler,
			CancellationToken token,
			[FromQuery] int skip = 0,
			[FromQuery] int take = 10) =>
		{
			return await handler.Handle(userId, skip, take, token);
		});
	}
}


public sealed class GetUserWithdrawalsHandler : IWalletServiceHandler
{
	private readonly ILogger<GetUserWithdrawalsHandler> _logger;
	private readonly IWalletRepository _walletRepository;

	public GetUserWithdrawalsHandler(
		ILogger<GetUserWithdrawalsHandler> logger,
		IWalletRepository walletRepository)
	{
		_logger = logger;
		_walletRepository = walletRepository;
	}



	public async Task<Result<IEnumerable<WithdrawalResponse>, Error>> Handle(Guid UserId, int skip, int take, CancellationToken ct)
	{
		var withdrawals = await _withdrawalService.GetUserWithdrawalsAsync(userId, skip, take);

		return Ok(new
		{
			withdrawals = withdrawals.Select(w => new
			{
				withdrawalId = w.Id,
				requestedAmount = w.RequestedAmount,
				commission = w.Commission,
				actualAmount = w.ActualAmount,
				cardMask = w.CardMask,
				status = w.Status.ToString(),
				createdAt = w.CreatedAt
			}).ToList(),
			total = withdrawals.Count
		});
	}

}