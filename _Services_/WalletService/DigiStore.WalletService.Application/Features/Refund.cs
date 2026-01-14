using CSharpFunctionalExtensions;
using DigiStore.Framework.Endpoints;
using DigiStore.SharedKernel;
using DigiStore.WalletService.Application.Extensions;
using DigiStore.WalletService.Application.Interfaces;
using DigiStore.WalletService.Contracts.Responses;
using DigiStore.WalletService.Domain;
using DigiStore.WalletService.Domain.Enums;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

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


public sealed class RefundHandler
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


	public async Task<Result<TransactionResponse, Error>> Handle(Guid userId, string orderId, decimal amount, CancellationToken ct = default)
	{
		try
		{
			if (amount <= 0)
			{
				return WalletErrors.InvalidAmount;
			}
			var wallet = await _walletRepository.GetByUserIdAsync(userId, ct);
			if (wallet == null)
			{
				return WalletErrors.WalletNotFound;
			}
			wallet.Deposit(amount);
			var transaction = new Transaction
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
			await _walletRepository.UpdateAsync(wallet, ct);
			await _walletRepository.AddTransactionAsync(transaction, ct);
			
			//await InvalidateWalletCacheAsync(userId, ct);
			
			_logger.LogInformation(
			"Refund successful for user {UserId}: Order {OrderId}, Amount: {Amount}", userId, orderId, amount);
			return transaction.MapToResponse();
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error processing refund for user: {UserId}", userId);
			return
			Error.Internal("wallet.refund_error", "Error processing refund for user");
		}
	}

}