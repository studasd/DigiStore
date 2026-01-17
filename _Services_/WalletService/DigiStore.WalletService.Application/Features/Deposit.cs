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


public record DepositCommand(Guid UserId, decimal Amount, string Description, string? PaymentMethod, string? ReferenceId = null);



public sealed class Deposit : IEndpoint
{
	//[Authorize]
	public void MapEndpoint(IEndpointRouteBuilder app)
	{
		app.MapPost("deposit/{userId}", async Task<EndpointResult<TransactionResponse>> (
			[FromRoute] Guid userId,
			[FromBody] DepositRequest request,
			[FromServices] DepositHandler handler,
			CancellationToken token) =>
		{
			var command = new DepositCommand(userId, request.Amount, request.Description, request.PaymentMethod);

			return await handler.Handle(command, token);
		});
	}
}


public sealed class DepositHandler : IWalletServiceHandler
{
	private readonly ILogger<DepositHandler> _logger;
	private readonly IWalletRepository _walletRepository;

	public DepositHandler(
		ILogger<DepositHandler> logger,
		IWalletRepository walletRepository)
	{
		_logger = logger;
		_walletRepository = walletRepository;
	}


	public async Task<Result<TransactionResponse, Error>> Handle(DepositCommand command, CancellationToken token)
	{
		try
		{
			if (command.Amount <= 0)
			{
				return WalletErrors.InvalidAmount;
			}
			
			var walletResult = await _walletRepository.GetOrCreateByUserIdAsync(command.UserId, token);
			if (walletResult.IsFailure)
			{
				return walletResult.Error;
			}

			var wallet = walletResult.Value;
			if (wallet.IsFrozen)
			{
				return WalletErrors.WalletFrozen;
			}

			wallet.Deposit(command.Amount);

			var transaction = new TransactionDS
			{
				Id = Guid.NewGuid(),
				WalletId = wallet.Id,
				UserId = command.UserId,
				Amount = command.Amount,
				Type = TransactionTypes.Deposit,
				Status = TransactionStatuses.Completed,
				Description = command.Description,
				BalanceAfter = wallet.Balance,
				PaymentMethod = command.PaymentMethod,
				ReferenceId = command.ReferenceId
			};
			await _walletRepository.UpdateAsync(wallet, token);
			await _walletRepository.AddTransactionAsync(transaction, token);

			//await InvalidateWalletCacheAsync(command.UserId, ct);
			
			_logger.LogInformation("Deposit successful for user {UserId}: {Amount} {Currency}", command.UserId, command.Amount, wallet.Currency.ToString());
			
			return transaction.MapToResponse();
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error depositing for user: {UserId}", command.UserId);
			return Error.Internal("wallet.deposit_error", "Error depositing for user");
		}
	}

}