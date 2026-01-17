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

namespace DigiStore.WalletService.Application.Features.Withdrawals;

/// Создать выплату (вывести на карту)
public sealed class CreateWithdrawal : IEndpoint
{
	//[Authorize]
	public void MapEndpoint(IEndpointRouteBuilder app)
	{
		app.MapPost("createWithdrawal/{userId}", async Task<EndpointResult<CreateWithdrawalResponse>> (
			[FromRoute] Guid userId,
			[FromBody] CreateWithdrawalRequest request,
			[FromServices] CreateWithdrawalHandler handler,
			CancellationToken token) =>
		{
			return await handler.Handle(userId, request.WalletId, request.Amount, request.CardNumber, token);
		});
	}
}


public sealed class CreateWithdrawalHandler : IWalletServiceHandler
{
	private readonly ILogger<CreateWithdrawalHandler> _logger;
	private readonly IWalletRepository _walletRepository;

	public CreateWithdrawalHandler(
		ILogger<CreateWithdrawalHandler> logger,
		IWalletRepository walletRepository)
	{
		_logger = logger;
		_walletRepository = walletRepository;
	}



	public async Task<Result<CreateWithdrawalResponse, Error>> Handle(Guid UserId, Guid WalletId, decimal Amount, string CardNumber, CancellationToken ct)
	{
		if (string.IsNullOrEmpty(request.WalletId) || !Guid.TryParse(request.WalletId, out var walletId))
			return BadRequest(new { error = "Некорректный WalletId" });

		if (request.Amount <= 0)
			return BadRequest(new { error = "Сумма должна быть больше 0" });

		if (string.IsNullOrEmpty(request.CardNumber))
			return BadRequest(new { error = "Номер карты не может быть пустым" });

		var (success, withdrawal, error) = await _withdrawalService.CreateWithdrawalAsync(
			walletId,
			userId,
			request.Amount,
			request.CardNumber);

		if (!success)
			return BadRequest(new { error });

		return Ok(new CreateWithdrawalResponse
		(
			withdrawal!.Id,
			withdrawal.RequestedAmount,
			withdrawal.Commission,
			withdrawal.ActualAmount,
			withdrawal.CardMask,
			withdrawal.Status
		));
	}

}