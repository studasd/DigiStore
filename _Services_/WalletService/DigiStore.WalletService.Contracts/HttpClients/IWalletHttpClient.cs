using CSharpFunctionalExtensions;
using StudCoreKit.SharedKernel;
using DigiStore.WalletService.Contracts.Requests;
using DigiStore.WalletService.Contracts.Requests.Payments;
using DigiStore.WalletService.Contracts.Responses;
using DigiStore.WalletService.Contracts.Responses.Payments;

namespace DigiStore.WalletService.Contracts.HttpClients;

public interface IWalletHttpClient
{
	Task<Result<CheckBalanceResponse, Error>> CheckBalanceAsync(Guid userId, decimal amount, CancellationToken token);

	Task<Result<TransactionResponse, Error>> DepositAsync(Guid userId, DepositRequest request, CancellationToken token);

	Task<UnitResult<Error>> FreezeWalletAsync(Guid userId, CancellationToken token);

	Task<UnitResult<Error>> UnfreezeWalletAsync(Guid userId, CancellationToken token);

	Task<Result<BalanceResponse, Error>> GetBalanceAsync(Guid userId, CancellationToken token);

	Task<Result<IEnumerable<TransactionResponse>, Error>> GetTransactionsAsync(Guid userId, int skip = 0, int take = 20, CancellationToken token = default);

	Task<Result<WalletResponse, Error>> GetWalletAsync(Guid userId, CancellationToken token);

	Task<Result<TransactionResponse, Error>> PurchaseAsync(Guid userId, PurchaseRequest request, CancellationToken token);

	Task<Result<TransactionResponse, Error>> RefundAsync(Guid userId, string orderId, decimal amount, CancellationToken token);

	Task<Result<TransactionResponse, Error>> WithdrawAsync(Guid userId, WithdrawRequest request, CancellationToken token);

	Task<Result<CreatePaymentResponse, Error>> CreatePaymentAsync(Guid userId, CreatePaymentRequest request, CancellationToken token);
}