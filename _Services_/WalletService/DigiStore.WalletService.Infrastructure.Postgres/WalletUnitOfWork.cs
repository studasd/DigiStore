using CSharpFunctionalExtensions;
using DigiStore.SharedKernel;
using DigiStore.WalletService.Application.Interfaces;
using DigiStore.WalletService.Infrastructure.Postgres.Data;
using Microsoft.EntityFrameworkCore.Storage;

namespace DigiStore.WalletService.Infrastructure.Postgres;

public sealed class WalletUnitOfWork : IWalletUnitOfWork
{
	private readonly WalletDbContext _context;

	public WalletUnitOfWork(WalletDbContext context)
	{
		_context = context;
	}

	public async Task<Result<IWalletDbTransaction, Error>> BeginTransactionAsync(CancellationToken token)
	{
		try
		{
			var tx = await _context.Database.BeginTransactionAsync(token);
			return new WalletDbTransaction(tx);
		}
		catch (Exception ex)
		{
			return Error.Failure("db.tx.begin_failed", ex.Message);
		}
	}
}



internal sealed class WalletDbTransaction : IWalletDbTransaction
{
	private readonly IDbContextTransaction _tx;

	public WalletDbTransaction(IDbContextTransaction tx)
	{
		_tx = tx;
	}

	public Task CommitAsync(CancellationToken token) => _tx.CommitAsync(token);
	public Task RollbackAsync(CancellationToken token) => _tx.RollbackAsync(token);
	public ValueTask DisposeAsync() => _tx.DisposeAsync();
}