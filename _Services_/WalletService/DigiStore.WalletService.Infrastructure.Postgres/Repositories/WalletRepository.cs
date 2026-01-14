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


	public async Task<Wallet?> GetByUserIdAsync(Guid userId, CancellationToken ct = default)
	{
		return await _context.Wallets
			.Include(w => w.Transactions)
			.FirstOrDefaultAsync(w => w.UserId == userId, ct);
	}

	public async Task<Wallet?> GetByIdAsync(Guid walletId, CancellationToken ct = default)
	{
		return await _context.Wallets
			.FirstOrDefaultAsync(w => w.Id == walletId, ct);
	}

	public async Task AddAsync(Wallet wallet, CancellationToken ct = default)
	{
		_context.Wallets.Add(wallet);
		await _context.SaveChangesAsync(ct);
		_logger.LogInformation("Wallet created: {WalletId}", wallet.Id);
	}

	public async Task UpdateAsync(Wallet wallet, CancellationToken ct = default)
	{
		_context.Wallets.Update(wallet);
		await _context.SaveChangesAsync(ct);
		_logger.LogInformation("Wallet updated: {WalletId}", wallet.Id);
	}

	public async Task<Transaction?> GetTransactionByIdAsync(Guid transactionId, CancellationToken ct = default)
	{
		return await _context.Transactions
			.FirstOrDefaultAsync(t => t.Id == transactionId, ct);
	}

	public async Task AddTransactionAsync(Transaction transaction, CancellationToken ct = default)
	{
		_context.Transactions.Add(transaction);
		await _context.SaveChangesAsync(ct);
		_logger.LogInformation("Transaction created: {TransactionId}", transaction.Id);
	}

	public async Task<IEnumerable<Transaction>> GetTransactionsByWalletIdAsync(
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