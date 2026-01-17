using CSharpFunctionalExtensions;
using DigiStore.SharedKernel;
using DigiStore.WalletService.Application.Interfaces;
using DigiStore.WalletService.Domain;
using DigiStore.WalletService.Infrastructure.Postgres.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DigiStore.WalletService.Infrastructure.Postgres.Repositories;

public class WalletRepository : IWalletRepository
{
	private readonly WalletDbContext _context;
	private readonly ILogger<WalletRepository> _logger;

	public WalletRepository(WalletDbContext context, ILogger<WalletRepository> logger)
	{
		_context = context;
		_logger = logger;
	}


	public async Task<Result<WalletDS, Error>> GetOrCreateByUserIdAsync(Guid userId, CancellationToken token)
	{
		var wallet = await _context.Wallets
				.Include(w => w.Transactions)
				.FirstOrDefaultAsync(w => w.UserId == userId, token);

		if (wallet != null)
			return wallet;

		// Wallet not found - create one (Id usually equals UserId)
		var newWallet = WalletDS.Create(userId);

		_context.Wallets.Add(newWallet);
		
		var saveResult = await SaveChangesAsync(token);
		if (saveResult.IsFailure)
			return saveResult.Error;

		_logger.LogInformation("Wallet created for user: {UserId} WalletId: {WalletId}", userId, newWallet.Id);
		
		return newWallet;
	}

	public async Task<Result<WalletDS, Error>> GetByIdAsync(Guid walletId, CancellationToken token)
	{
		var wallet = await _context.Wallets
			.FirstOrDefaultAsync(w => w.Id == walletId, token);

		if(wallet == null) 
			return Error.NotFound("wallet.not", "Кошелек не найден") ;

		return wallet;
	}

	public async Task<UnitResult<Error>> AddAsync(WalletDS wallet, CancellationToken token)
	{
		_context.Wallets.Add(wallet);

		var saveResult = await SaveChangesAsync(token);
		if (saveResult.IsFailure)
			return saveResult.Error;

		_logger.LogInformation("Wallet created: {WalletId}", wallet.Id);

		return Result.Success<Error>();
	}

	public async Task<UnitResult<Error>> UpdateAsync(WalletDS wallet, CancellationToken token)
	{
		_context.Wallets.Update(wallet);

		var saveResult = await SaveChangesAsync(token);
		if (saveResult.IsFailure)
			return saveResult.Error;

		_logger.LogInformation("Wallet updated: {WalletId}", wallet.Id);
	
		return Result.Success<Error>();
	}

	public async Task<Result<TransactionDS, Error>> GetTransactionByIdAsync(Guid transactionId, CancellationToken token)
	{
		var transaction = await _context.Transactions
			.FirstOrDefaultAsync(t => t.Id == transactionId, token);

		if(transaction == null)
			return Error.NotFound("transaction.not", "Транзакция не найдена");

		return transaction;
	}

	public async Task<UnitResult<Error>> AddTransactionAsync(TransactionDS transaction, CancellationToken token)
	{
		_context.Transactions.Add(transaction);

		var saveResult = await SaveChangesAsync(token);
		if (saveResult.IsFailure)
			return saveResult.Error;

		_logger.LogInformation("Transaction created: {TransactionId}", transaction.Id);
	
		return Result.Success<Error>();
	}

	public async Task<Result<IEnumerable<TransactionDS>, Error>> GetTransactionsByWalletIdAsync(
		Guid walletId,
		int skip = 0,
		int take = 20,
		CancellationToken token = default)
	{
		var transactions = await _context.Transactions
			.Where(t => t.WalletId == walletId)
			.OrderByDescending(t => t.CreatedAt)
			.Skip(skip)
			.Take(take)
			.ToListAsync(token);

		return transactions;
	}

	public async Task<Result<int, Error>> GetTransactionCountAsync(Guid walletId, CancellationToken token)
	{
		return await _context.Transactions
			.CountAsync(t => t.WalletId == walletId, token);
	}


	public async Task<UnitResult<Error>> SaveChangesAsync(CancellationToken token)
	{
		try
		{
			await _context.SaveChangesAsync(token);
		}
		catch (DbUpdateException ex)
		{
			_logger.LogWarning(ex, "Failed save changes");

			return Error.Failure("failed.db.savechange", $"Failed save changes");
		}

		return Result.Success<Error>();
	}
}