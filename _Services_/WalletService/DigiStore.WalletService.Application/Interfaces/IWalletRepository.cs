using CSharpFunctionalExtensions;
using DigiStore.SharedKernel;
using DigiStore.WalletService.Domain;

namespace DigiStore.WalletService.Application.Interfaces;

public interface IWalletRepository
{
	Task<Result<WalletDS, Error>> GetOrCreateByUserIdAsync(Guid userId, CancellationToken ct = default);
	Task<WalletDS?> GetByIdAsync(Guid walletId, CancellationToken ct = default);
	Task AddAsync(WalletDS wallet, CancellationToken ct = default);
	Task UpdateAsync(WalletDS wallet, CancellationToken ct = default);
	Task<TransactionDS?> GetTransactionByIdAsync(Guid transactionId, CancellationToken ct = default);
	Task AddTransactionAsync(TransactionDS transaction, CancellationToken ct = default);
	Task<IEnumerable<TransactionDS>> GetTransactionsByWalletIdAsync(Guid walletId, int skip = 0, int take = 20, CancellationToken ct = default);
	Task<int> GetTransactionCountAsync(Guid walletId, CancellationToken ct = default);
}