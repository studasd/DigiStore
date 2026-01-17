using CSharpFunctionalExtensions;
using DigiStore.SharedKernel;
using DigiStore.WalletService.Domain;

namespace DigiStore.WalletService.Application.Interfaces;

public interface IWalletRepository
{
	Task<Result<WalletDS, Error>> GetOrCreateByUserIdAsync(Guid userId, CancellationToken ct);
	Task<Result<WalletDS, Error>> GetByIdAsync(Guid walletId, CancellationToken ct);
	Task<UnitResult<Error>> AddAsync(WalletDS wallet, CancellationToken ct);
	Task<UnitResult<Error>> UpdateAsync(WalletDS wallet, CancellationToken ct);
	Task<Result<TransactionDS, Error>> GetTransactionByIdAsync(Guid transactionId, CancellationToken ct);
	Task<UnitResult<Error>> AddTransactionAsync(Result<TransactionDS, Error> transaction, CancellationToken ct);
	Task<Result<IEnumerable<TransactionDS>, Error>> GetTransactionsByWalletIdAsync(Guid walletId, int skip = 0, int take = 20, CancellationToken ct = default);
	Task<Result<int, Error>> GetTransactionCountAsync(Guid walletId, CancellationToken ct);
}