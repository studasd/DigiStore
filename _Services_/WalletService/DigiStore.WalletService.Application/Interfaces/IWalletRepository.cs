using DigiStore.WalletService.Domain;

namespace DigiStore.WalletService.Application.Interfaces;

public interface IWalletRepository
{
	Task<Wallet?> GetByUserIdAsync(Guid userId, CancellationToken ct = default);
	Task<Wallet?> GetByIdAsync(Guid walletId, CancellationToken ct = default);
	Task AddAsync(Wallet wallet, CancellationToken ct = default);
	Task UpdateAsync(Wallet wallet, CancellationToken ct = default);
	Task<Transaction?> GetTransactionByIdAsync(Guid transactionId, CancellationToken ct = default);
	Task AddTransactionAsync(Transaction transaction, CancellationToken ct = default);
	Task<IEnumerable<Transaction>> GetTransactionsByWalletIdAsync(Guid walletId, int skip = 0, int take = 20, CancellationToken ct = default);
	Task<int> GetTransactionCountAsync(Guid walletId, CancellationToken ct = default);
}