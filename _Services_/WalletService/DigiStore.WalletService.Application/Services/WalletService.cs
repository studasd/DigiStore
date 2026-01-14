using CSharpFunctionalExtensions;
using DigiStore.SharedKernel;
using DigiStore.WalletService.Application.Commands;
using DigiStore.WalletService.Application.Interfaces;
using DigiStore.WalletService.Contracts.Responses;
using DigiStore.WalletService.Domain;
using DigiStore.WalletService.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace DigiStore.WalletService.Application.Services;

public class WalletService : IWalletService
{
	private readonly IWalletRepository _repository;
	//private readonly ICacheService _cache;
	private readonly ILogger<WalletService> _logger;
	private const string WalletCacheKeyFormat = "wallet:{0}";
	private const string BalanceCacheKeyFormat = "wallet:balance:{0}";
	private readonly TimeSpan _walletCacheExpiration = TimeSpan.FromMinutes(5);

	public WalletService(
		IWalletRepository repository,
		//ICacheService cache,
		ILogger<WalletService> logger)
	{
		_repository = repository;
		//_cache = cache;
		_logger = logger;
	}


	public async Task<Result<WalletResponse, Error>> GetWalletAsync(Guid userId, CancellationToken ct = default)
	{
		try
		{
			//var cacheKey = string.Format(WalletCacheKeyFormat, userId);
			//var cached = await _cache.GetAsync<WalletResponse>(cacheKey, ct);
			//if (cached != null)
			//{
			//	return Result<WalletResponse>.Success(cached);
			//}
			var wallet = await _repository.GetByUserIdAsync(userId, ct);
			if (wallet == null)
			{
				_logger.LogWarning("Wallet not found for user: {UserId}", userId);
				return WalletErrors.WalletNotFound;
			}
			var response = MapToResponse(wallet);
			//await _cache.SetAsync(cacheKey, response, _walletCacheExpiration, ct);
			return response;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error getting wallet for user: {UserId}", userId);
			return Error.Internal("wallet.retrieval_error", "Error getting wallet");
		}
	}


	public async Task<Result<decimal, Error>> GetBalanceAsync(Guid userId, CancellationToken ct = default)
	{
		try
		{
			var cacheKey = string.Format(BalanceCacheKeyFormat, userId);
			//var cached = await _cache.GetAsync<decimal?>(cacheKey, ct);
			//if (cached.HasValue)
			//{
			//	return Result<decimal>.Success(cached.Value);
			//}
			var wallet = await _repository.GetByUserIdAsync(userId, ct);
			if (wallet == null)
			{
				return WalletErrors.WalletNotFound;
			}
			//await _cache.SetAsync(cacheKey, wallet.Balance, TimeSpan.FromMinutes(1), ct);
			return wallet.Balance;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error getting balance for user: {UserId}", userId);
			return Error.Internal("wallet.balance_error", "Error getting balance");
		}
	}

	public async Task<Result<bool, Error>> HasSufficientBalanceAsync(Guid userId, decimal amount, CancellationToken ct = default)
	{
		if (amount <= 0)
		{
			return WalletErrors.InvalidAmount;
		}
		var balanceResult = await GetBalanceAsync(userId, ct);
		if (balanceResult.IsFailure)
		{
			return balanceResult.Error;
		}
		return balanceResult.Value >= amount;
	}


	public async Task<Result<TransactionResponse, Error>> DepositAsync(DepositCommand command, CancellationToken ct = default)
	{
		try
		{
			if (command.Amount <= 0)
			{
				return WalletErrors.InvalidAmount;
			}
			var wallet = await _repository.GetByUserIdAsync(command.UserId, ct);
			if (wallet == null)
			{
				// Create new wallet for new user
				wallet = new Wallet
				{
					Id = Guid.NewGuid(),
					UserId = command.UserId,
					Balance = 0,
					Currency = "RUB"
				};
				await _repository.AddAsync(wallet, ct);
			}
			wallet.Deposit(command.Amount);
			var transaction = new Transaction
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
			await _repository.UpdateAsync(wallet, ct);
			await _repository.AddTransactionAsync(transaction, ct);
			await InvalidateWalletCacheAsync(command.UserId, ct);
			_logger.LogInformation("Deposit successful for user {UserId}: {Amount} {Currency}", command.UserId, command.Amount, wallet.Currency);
			return MapTransactionToResponse(transaction);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error depositing for user: {UserId}", command.UserId);
			return Error.Internal("wallet.deposit_error", "Error depositing for user");
		}
	}

	public async Task<Result<TransactionResponse, Error>> WithdrawAsync(WithdrawCommand command, CancellationToken ct = default)
	{
		try
		{
			if (command.Amount <= 0)
			{
				return WalletErrors.InvalidAmount;
			}
			var wallet = await _repository.GetByUserIdAsync(command.UserId, ct);
			if (wallet == null)
			{
				return WalletErrors.WalletNotFound;
			}
			if (wallet.IsFrozen)
			{
				return WalletErrors.WalletFrozen;
			}
			if (!wallet.HasSufficientBalance(command.Amount))
			{
				return WalletErrors.InsufficientBalance;
			}
			wallet.Withdraw(command.Amount);
			var transaction = new Transaction
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
			await _repository.UpdateAsync(wallet, ct);
			await _repository.AddTransactionAsync(transaction, ct);
			await InvalidateWalletCacheAsync(command.UserId, ct);
			_logger.LogInformation("Withdrawal successful for user {UserId}: {Amount}", command.UserId, command.Amount);
			return MapTransactionToResponse(transaction);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error withdrawing for user: {UserId}", command.UserId);
			return Error.Internal("wallet.withdrawal_error", "Error withdrawing for user");
		}
	}

	public async Task<Result<TransactionResponse, Error>> PurchaseAsync(PurchaseCommand command, CancellationToken ct = default)
	{
		try
		{
			var wallet = await _repository.GetByUserIdAsync(command.UserId, ct);
			if (wallet == null)
			{
				return WalletErrors.WalletNotFound;
			}
			if (!wallet.HasSufficientBalance(command.Amount))
			{
				return WalletErrors.InsufficientBalance;
			}
			wallet.Withdraw(command.Amount);
			var transaction = new Transaction
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
			await _repository.UpdateAsync(wallet, ct);
			await _repository.AddTransactionAsync(transaction, ct);
			await InvalidateWalletCacheAsync(command.UserId, ct);
			_logger.LogInformation("Purchase successful for user {UserId}: Order {OrderId}, Amount: {Amount}", command.UserId, command.OrderId, command.Amount);
			return MapTransactionToResponse(transaction);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error processing purchase for user: {UserId}", command.UserId);
			return Error.Internal("wallet.purchase_error", "Error processing purchase for user");
		}
	}

	public async Task<Result<TransactionResponse, Error>> RefundAsync(
		Guid userId,
		decimal amount,
		string orderId,
		CancellationToken ct = default)
	{
		try
		{
			if (amount <= 0)
			{
				return WalletErrors.InvalidAmount;
			}
			var wallet = await _repository.GetByUserIdAsync(userId, ct);
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
			await _repository.UpdateAsync(wallet, ct);
			await _repository.AddTransactionAsync(transaction, ct);
			await InvalidateWalletCacheAsync(userId, ct);
			_logger.LogInformation(
			"Refund successful for user {UserId}: Order {OrderId}, Amount: {Amount}", userId, orderId, amount);
			return MapTransactionToResponse(transaction);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error processing refund for user: {UserId}", userId);
			return 
			Error.Internal("wallet.refund_error", "Error processing refund for user");
		}
	}

	public async Task<Result<IEnumerable<TransactionResponse>, Error>> GetTransactionsAsync(
		Guid userId,
		int skip = 0,
		int take = 20,
		CancellationToken ct = default)
	{
		try
		{
			var wallet = await _repository.GetByUserIdAsync(userId, ct);
			if (wallet == null)
			{
				return WalletErrors.WalletNotFound;
			}
			var transactions = await _repository.GetTransactionsByWalletIdAsync(
			wallet.Id, skip, take, ct);
			var response = transactions.Select(MapTransactionToResponse).ToList();
			return response;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error getting transactions for user: {UserId}", userId);
			return Error.Internal("wallet.transactions_error", "Error getting transactions for user");
		}
	}

	public async Task<UnitResult<Error>> FreezeWalletAsync(Guid userId, CancellationToken ct = default)
	{
		try
		{
			var wallet = await _repository.GetByUserIdAsync(userId, ct);
			if (wallet == null)
			{
				return WalletErrors.WalletNotFound;
			}
			wallet.Freeze();
			await _repository.UpdateAsync(wallet, ct);
			await InvalidateWalletCacheAsync(userId, ct);
			_logger.LogInformation("Wallet frozen for user: {UserId}", userId);
			return Result.Success<Error>();
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error freezing wallet for user: {UserId}", userId);
			return Error.Internal("wallet.freeze_error", "Error freezing wallet for user");
		}
	}

	public async Task<UnitResult<Error>> UnfreezeWalletAsync(Guid userId, CancellationToken ct = default)
	{
		try
		{
			var wallet = await _repository.GetByUserIdAsync(userId, ct);
			if (wallet == null)
			{
				return WalletErrors.WalletNotFound;
			}
			wallet.Unfreeze();
			await _repository.UpdateAsync(wallet, ct);
			await InvalidateWalletCacheAsync(userId, ct);
			_logger.LogInformation("Wallet unfrozen for user: {UserId}", userId);
			return Result.Success<Error>();
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error unfreezing wallet for user: {UserId}", userId);
			return Error.Internal("wallet.unfreeze_error", "Error unfreezing wallet for user");
		}
	}

	private WalletResponse MapToResponse(Wallet wallet)
	{
		return new WalletResponse
		{
			Id = wallet.Id,
			UserId = wallet.UserId,
			Balance = wallet.Balance,
			TotalDeposited = wallet.TotalDeposited,
			TotalWithdrawn = wallet.TotalWithdrawn,
			Currency = wallet.Currency,
			IsFrozen = wallet.IsFrozen,
			CreatedAt = wallet.CreatedAt,
			UpdatedAt = wallet.UpdatedAt
		};
	}

	private TransactionResponse MapTransactionToResponse(Transaction transaction)
	{
		return new TransactionResponse
		{
			Id = transaction.Id,
			WalletId = transaction.WalletId,
			Amount = transaction.Amount,
			Type = transaction.Type.ToString(),
			Status = transaction.Status.ToString(),
			Description = transaction.Description,
			BalanceAfter = transaction.BalanceAfter,
			CreatedAt = transaction.CreatedAt
		};
	}

	private async Task InvalidateWalletCacheAsync(Guid userId, CancellationToken ct)
	{
		//await _cache.RemoveAsync(string.Format(WalletCacheKeyFormat, userId), ct);
		//await _cache.RemoveAsync(string.Format(BalanceCacheKeyFormat, userId), ct);
	}
}