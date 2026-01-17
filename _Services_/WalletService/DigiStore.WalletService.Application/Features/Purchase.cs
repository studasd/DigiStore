using CSharpFunctionalExtensions;
using DigiStore.Framework.Endpoints;
using DigiStore.SharedKernel;
using DigiStore.WalletService.Application.Extensions;
using DigiStore.WalletService.Application.Interfaces;
using DigiStore.WalletService.Contracts.Requests;
using DigiStore.WalletService.Contracts.Responses;
using DigiStore.WalletService.Domain;
using DigiStore.WalletService.Domain.Enums;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;

namespace DigiStore.WalletService.Application.Features;

public record PurchaseCommand(Guid UserId, decimal Amount, string OrderId, string Description);


public sealed class Purchase : IEndpoint
{
	//[Authorize]
	public void MapEndpoint(IEndpointRouteBuilder app)
	{
		app.MapPost("purchase/{userId}", async Task<EndpointResult<TransactionResponse>> (
			[FromRoute] Guid userId,
			[FromBody] PurchaseRequest request,
			[FromServices] PurchaseHandler handler,
			CancellationToken token) =>
		{
			var command = new PurchaseCommand(userId, request.Amount, request.OrderId, request.Description);

			return await handler.Handle(command, token);
		});
	}
}


public sealed class PurchaseHandler : IWalletServiceHandler
{
	private readonly ILogger<PurchaseHandler> _logger;
	private readonly IWalletRepository _walletRepository;

	public PurchaseHandler(
		ILogger<PurchaseHandler> logger,
		IWalletRepository walletRepository)
	{
		_logger = logger;
		_walletRepository = walletRepository;
	}


	public async Task<Result<TransactionResponse, Error>> Handle(PurchaseCommand command, CancellationToken token)
	{
		try
		{
			var walletResult = await _walletRepository.GetOrCreateByUserIdAsync(command.UserId, token);
			if (walletResult.IsFailure)
				return walletResult.Error;

			var wallet = walletResult.Value;

			if (!wallet.HasSufficientBalance(command.Amount))
			{
				return WalletErrors.InsufficientBalance;
			}

			wallet.Withdraw(command.Amount);
			var transaction = new TransactionDS
			{
				Id = Guid.NewGuid(),
				WalletId = wallet.Id,
				UserId = command.UserId,
				Amount = command.Amount,
				Type = TransactionTypes.Purchase,
				Status = TransactionStatuses.Completed,
				Description = command.Description,
				BalanceAfter = wallet.Balance,
				ReferenceId = command.OrderId,
				ReferenceType = "Order"
			};

			await _walletRepository.UpdateAsync(wallet, token);
			await _walletRepository.AddTransactionAsync(transaction, token);
			
			//await InvalidateWalletCacheAsync(command.UserId, ct);
			
			_logger.LogInformation("Purchase successful for user {UserId}: Order {OrderId}, Amount: {Amount}", command.UserId, command.OrderId, command.Amount);
			
			return transaction.MapToResponse();
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error processing purchase for user: {UserId}", command.UserId);
			return Error.Internal("wallet.purchase_error", "Error processing purchase for user");
		}
	}

}