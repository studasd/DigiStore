using CSharpFunctionalExtensions;
using DigiStore.SharedKernel;
using DigiStore.WalletService.Contracts.Requests;
using DigiStore.WalletService.Contracts.Responses;

namespace DigiStore.WalletService.Contracts.HttpClients;

public interface IWalletHttpClient
{
	Task<Result<CheckBalanceResponse, Error>> CheckBalanceAsync(Guid userId, decimal amount, CancellationToken cancellationToken);

	Task<Result<TransactionResponse, Error>> DepositAsync(Guid userId, DepositRequest request, CancellationToken cancellationToken);

	Task<UnitResult<Error>> FreezeWalletAsync(Guid userId, CancellationToken cancellationToken);

	Task<UnitResult<Error>> UnfreezeWalletAsync(Guid userId, CancellationToken cancellationToken);

	Task<Result<BalanceResponse, Error>> GetBalanceAsync(Guid userId, CancellationToken cancellationToken);

	Task<Result<IEnumerable<TransactionResponse>, Error>> GetTransactionsAsync(Guid userId, int skip = 0, int take = 20, CancellationToken cancellationToken = default);

	Task<Result<WalletResponse, Error>> GetWalletAsync(Guid userId, CancellationToken cancellationToken);

	Task<Result<TransactionResponse, Error>> PurchaseAsync(Guid userId, PurchaseRequest request, CancellationToken cancellationToken);

	Task<Result<TransactionResponse, Error>> RefundAsync(Guid userId, string orderId, decimal amount, CancellationToken cancellationToken);

	Task<Result<TransactionResponse, Error>> WithdrawAsync(Guid userId, WithdrawRequest request, CancellationToken cancellationToken);
}