using CSharpFunctionalExtensions;
using DigiStore.Enums;
using DigiStore.WalletService.Application.Extensions;
using DigiStore.WalletService.Application.Interfaces;
using DigiStore.WalletService.Contracts.Responses;
using DigiStore.WalletService.Domain;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using StudCoreKit.Framework.Endpoints;
using StudCoreKit.SharedKernel;

namespace DigiStore.WalletService.Application.Features;

public sealed class Refund : IEndpoint
{
	//[Authorize]
	public void MapEndpoint(IEndpointRouteBuilder app)
	{
		app.MapPost("refund/{userId}/{orderId}/{amount}", async Task<EndpointResult<TransactionResponse>> (
            [FromRoute] Guid userId,
			[FromRoute] string orderId,
			[FromRoute] decimal amount,
			[FromServices] RefundHandler handler,
			CancellationToken token) => await handler.Handle(userId, orderId, amount, token));
	}
}


public sealed class RefundHandler : IWalletServiceHandler
{
	private readonly ILogger<RefundHandler> _logger;
	private readonly IWalletRepository _walletRepository;

	public RefundHandler(
		ILogger<RefundHandler> logger,
		IWalletRepository walletRepository)
	{
		_logger = logger;
		_walletRepository = walletRepository;
	}


	public async Task<Result<TransactionResponse, Error>> Handle(Guid userId, string orderId, decimal amount, CancellationToken token)
	{
		if (amount <= 0)
			return WalletErrors.InvalidAmount;

		var walletResult = await _walletRepository.GetOrCreateByUserIdAsync(userId, token);
		if (walletResult.IsFailure)
			return walletResult.Error;

		var wallet = walletResult.Value;

		wallet.Deposit(amount);
		var transaction = new TransactionDS
		{
			Id = Guid.NewGuid(),
			WalletId = wallet.Id,
			UserId = userId,
			Amount = amount,
			Type = TransactionTypes.Refund,
			Status = TransactionStatuses.Completed,
			Description = $"Refund for order {orderId}",
			BalanceAfter = wallet.Balance,
			ReferenceId = orderId,
			ReferenceType = "Order"
		};
		var updateResult = await _walletRepository.UpdateAsync(wallet, token);
		if (updateResult.IsFailure)
			return updateResult.Error;

		var addResult = await _walletRepository.AddTransactionAsync(transaction, token);
		if (addResult.IsFailure)
			return addResult.Error;

		//await InvalidateWalletCacheAsync(userId, ct);

		_logger.LogInformation(
		"Refund successful for user {UserId}: Order {OrderId}, Amount: {Amount}", userId, orderId, amount);
		return transaction.MapToResponse();
	}

}