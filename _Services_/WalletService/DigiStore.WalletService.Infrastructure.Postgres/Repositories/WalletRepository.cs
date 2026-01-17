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


	public async Task<Result<WalletDS, Error>> GetOrCreateByUserIdAsync(Guid userId, CancellationToken ct = default)
	{
		var wallet = await _context.Wallets
				.Include(w => w.Transactions)
				.FirstOrDefaultAsync(w => w.UserId == userId, ct);

		if (wallet != null)
			return wallet;

		// Wallet not found - create one (Id usually equals UserId)
		var newWallet = WalletDS.Create(userId);

		_context.Wallets.Add(newWallet);
		try
		{
			await _context.SaveChangesAsync(ct);
			_logger.LogInformation("Wallet created for user: {UserId} WalletId: {WalletId}", userId, newWallet.Id);
		}
		catch (DbUpdateException ex)
		{
			// Possible concurrent creation by another process - try to reload
			_logger.LogWarning(ex, "Failed to create wallet for user {UserId}, reloading", userId);
			//return await _context.Wallets
			//		.Include(w => w.Transactions)
			//		.FirstOrDefaultAsync(w => w.UserId == userId, ct);

			return Error.Failure("failed.create.wallet", $"Failed to create wallet for user {userId}, reloading");
		}

		return newWallet;
	}

	public async Task<WalletDS?> GetByIdAsync(Guid walletId, CancellationToken ct = default)
	{
		return await _context.Wallets
			.FirstOrDefaultAsync(w => w.Id == walletId, ct);
	}

	public async Task AddAsync(WalletDS wallet, CancellationToken ct = default)
	{
		_context.Wallets.Add(wallet);
		await _context.SaveChangesAsync(ct);
		_logger.LogInformation("Wallet created: {WalletId}", wallet.Id);
	}

	public async Task UpdateAsync(WalletDS wallet, CancellationToken ct = default)
	{
		_context.Wallets.Update(wallet);
		await _context.SaveChangesAsync(ct);
		_logger.LogInformation("Wallet updated: {WalletId}", wallet.Id);
	}

	public async Task<TransactionDS?> GetTransactionByIdAsync(Guid transactionId, CancellationToken ct = default)
	{
		return await _context.Transactions
			.FirstOrDefaultAsync(t => t.Id == transactionId, ct);
	}

	public async Task AddTransactionAsync(TransactionDS transaction, CancellationToken ct = default)
	{
		_context.Transactions.Add(transaction);
		await _context.SaveChangesAsync(ct);
		_logger.LogInformation("Transaction created: {TransactionId}", transaction.Id);
	}

	public async Task<IEnumerable<TransactionDS>> GetTransactionsByWalletIdAsync(
		Guid walletId,
		int skip = 0,
		int take = 20,
		CancellationToken ct = default)
	{
		return await _context.Transactions
			.Where(t => t.WalletId == walletId)
			.OrderByDescending(t => t.CreatedAt)
			.Skip(skip)
			.Take(take)
			.ToListAsync(ct);
	}

	public async Task<int> GetTransactionCountAsync(Guid walletId, CancellationToken ct = default)
	{
		return await _context.Transactions
			.CountAsync(t => t.WalletId == walletId, ct);
	}
}