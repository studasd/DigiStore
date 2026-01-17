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

public record WithdrawCommand(Guid UserId, decimal Amount, string Description, string? ReferenceId = null);


public sealed class Withdraw : IEndpoint
{
	//[Authorize]
	public void MapEndpoint(IEndpointRouteBuilder app)
	{
		app.MapPost("withdraw/{userId}", async Task<EndpointResult<TransactionResponse>> (
			[FromRoute] Guid userId,
			[FromBody] WithdrawRequest request,
			[FromServices] WithdrawHandler handler,
			CancellationToken token) => 
		{
			var command = new WithdrawCommand(userId, request.Amount, request.Description);

			return await handler.Handle(command, token);
		});
	}
}


public sealed class WithdrawHandler : IWalletServiceHandler
{
	private readonly ILogger<WithdrawHandler> _logger;
	private readonly IWalletRepository _walletRepository;

	public WithdrawHandler(
		ILogger<WithdrawHandler> logger,
		IWalletRepository walletRepository)
	{
		_logger = logger;
		_walletRepository = walletRepository;
	}


	public async Task<Result<TransactionResponse, Error>> Handle(WithdrawCommand command, CancellationToken token)
	{
		if (command.Amount <= 0)
		{
			return WalletErrors.InvalidAmount;
		}
		var walletResult = await _walletRepository.GetOrCreateByUserIdAsync(command.UserId, token);
		if (walletResult.IsFailure)
			return walletResult.Error;

		var wallet = walletResult.Value;

		if (wallet.IsFrozen)
			return WalletErrors.WalletFrozen;

		if (!wallet.HasSufficientBalance(command.Amount))
			return WalletErrors.InsufficientBalance;

		wallet.Withdraw(command.Amount);
		var transaction = new TransactionDS
		{
			Id = Guid.NewGuid(),
			WalletId = wallet.Id,
			UserId = command.UserId,
			Amount = command.Amount,
			Type = TransactionTypes.Withdrawal,
			Status = TransactionStatuses.Completed,
			Description = command.Description,
			BalanceAfter = wallet.Balance,
			ReferenceId = command.ReferenceId
		};

		var updateResult = await _walletRepository.UpdateAsync(wallet, token);
		if (updateResult.IsFailure)
			return updateResult.Error;

		var addResult = await _walletRepository.AddTransactionAsync(transaction, token);
		if (addResult.IsFailure)
			return addResult.Error;

		//await InvalidateWalletCacheAsync(command.UserId, ct);
		_logger.LogInformation("Withdrawal successful for user {UserId}: {Amount}", command.UserId, command.Amount);
			
		return transaction.MapToResponse();
	}

}