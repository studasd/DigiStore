using CSharpFunctionalExtensions;
using StudCoreKit.SharedKernel;

namespace DigiStore.WalletService.Application.Interfaces;

public interface IWalletDbTransaction : IAsyncDisposable
{
	Task CommitAsync(CancellationToken token);
	Task RollbackAsync(CancellationToken token);
}

public interface IWalletUnitOfWork
{
	Task<Result<IWalletDbTransaction, Error>> BeginTransactionAsync(CancellationToken token);
}
