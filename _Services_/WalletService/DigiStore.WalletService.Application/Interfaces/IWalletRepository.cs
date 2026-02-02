using CSharpFunctionalExtensions;
using StudCoreKit.SharedKernel;
using DigiStore.WalletService.Domain;

namespace DigiStore.WalletService.Application.Interfaces;

public interface IWalletRepository
{
	Task<Result<WalletDS, Error>> GetOrCreateByUserIdAsync(Guid userId, CancellationToken token);

	Task<Result<WalletDS, Error>> GetByIdAsync(Guid walletId, CancellationToken token);

	Task<Result<WalletDS, Error>> GetByIdForUpdateAsync(Guid walletId, CancellationToken token);

	Task<UnitResult<Error>> AddAsync(WalletDS wallet, CancellationToken token);

	Task<UnitResult<Error>> UpdateAsync(WalletDS wallet, CancellationToken token);

	Task<Result<TransactionDS, Error>> GetTransactionByIdAsync(Guid transactionId, CancellationToken token);

	Task<UnitResult<Error>> AddTransactionAsync(TransactionDS transaction, CancellationToken token);

	Task<Result<IEnumerable<TransactionDS>, Error>> GetTransactionsByWalletIdAsync(Guid walletId, int skip = 0, int take = 20, CancellationToken token = default);

	Task<Result<int, Error>> GetTransactionCountAsync(Guid walletId, CancellationToken token);

	Task<UnitResult<Error>> SaveChangesAsync(CancellationToken token);
}