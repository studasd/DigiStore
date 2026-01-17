using CSharpFunctionalExtensions;
using DigiStore.Framework.Endpoints;
using DigiStore.SharedKernel;
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
    private readonly IWithdrawalRepository _withdrawalRepository;

	public GetUserWithdrawalsHandler(
		ILogger<GetUserWithdrawalsHandler> logger,
		IWithdrawalRepository withdrawalRepository)
	{
		_logger = logger;
        _withdrawalRepository = withdrawalRepository;
	}



	public async Task<Result<IEnumerable<WithdrawalResponse>, Error>> Handle(Guid userId, int skip, int take, CancellationToken ct)
	{
		var withdrawals = await _withdrawalRepository.GetUserWithdrawalsAsync(userId, skip, take, ct);

		if (withdrawals.IsFailure)
			return withdrawals.Error;

		return withdrawals.Value.Select(w => new WithdrawalResponse
		(
			w.Id,
			w.RequestedAmount,
			w.Commission,
			w.ActualAmount,
			w.CardMask,
			w.Status,
			w.CreatedAt,
			w.CompletedAt
		)).ToList();
	}

}