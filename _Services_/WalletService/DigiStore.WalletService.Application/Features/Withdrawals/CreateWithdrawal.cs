using CSharpFunctionalExtensions;
using StudCoreKit.Framework.Endpoints;
using StudCoreKit.SharedKernel;
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
    private readonly IWithdrawalService _withdrawalService;
    private readonly IWalletRepository _walletRepository;

	public CreateWithdrawalHandler(
		ILogger<CreateWithdrawalHandler> logger,
		IWithdrawalService withdrawalService,
		IWalletRepository walletRepository)
	{
		_logger = logger;
        _withdrawalService = withdrawalService;
        _walletRepository = walletRepository;
	}



	public async Task<Result<CreateWithdrawalResponse, Error>> Handle(Guid userId, Guid walletId, decimal amount, string cardNumber, CancellationToken token)
	{
		if (amount <= 0)
			return Error.Validation("amount.bad", "Сумма должна быть больше 0");

		if (string.IsNullOrEmpty(cardNumber))
			return Error.NotFound("cardnumber.emty", "Номер карты не может быть пустым");

		var withdrawalResult = await _withdrawalService.CreateWithdrawalAsync(walletId, userId, amount, cardNumber, token);
		if(withdrawalResult.IsFailure)
			return withdrawalResult.Error;

		var withdrawal = withdrawalResult.Value;

		return new CreateWithdrawalResponse
		(
			withdrawal!.Id,
			withdrawal.RequestedAmount,
			withdrawal.Commission,
			withdrawal.ActualAmount,
			withdrawal.CardMask,
			withdrawal.Status
		);
	}

}