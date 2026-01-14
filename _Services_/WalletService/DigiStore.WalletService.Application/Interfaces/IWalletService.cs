using CSharpFunctionalExtensions;
using DigiStore.SharedKernel;
using DigiStore.WalletService.Application.Commands;
using DigiStore.WalletService.Contracts.Responses;

namespace DigiStore.WalletService.Application.Interfaces;

public interface IWalletService
{
	/// <summary>
	/// Get wallet by user ID
	/// </summary>
	Task<Result<WalletResponse, Error>> GetWalletAsync(Guid userId, CancellationToken ct = default);

	/// <summary>
	/// Get balance
	/// </summary>
	Task<Result<decimal, Error>> GetBalanceAsync(Guid userId, CancellationToken ct = default);

	/// <summary>
	/// Check if user has sufficient balance
	/// </summary>
	Task<Result<bool, Error>> HasSufficientBalanceAsync(Guid userId, decimal amount, CancellationToken ct = default);

	/// <summary>
	/// Deposit money
	/// </summary>
	Task<Result<TransactionResponse, Error>> DepositAsync(DepositCommand command, CancellationToken ct = default);

	/// <summary>
	/// Withdraw money
	/// </summary>
	Task<Result<TransactionResponse, Error>> WithdrawAsync(WithdrawCommand command, CancellationToken ct = default);

	/// <summary>
	/// Purchase (spend money)
	/// </summary>
	Task<Result<TransactionResponse, Error>> PurchaseAsync(PurchaseCommand command, CancellationToken ct = default);

	/// <summary>
	/// Refund (return money)
	/// </summary>
	Task<Result<TransactionResponse, Error>> RefundAsync(Guid userId, decimal amount, string orderId, CancellationToken ct = default);

	/// <summary>
	/// Get transaction history
	/// </summary>
	Task<Result<IEnumerable<TransactionResponse>, Error>> GetTransactionsAsync(Guid userId, int skip = 0, int take = 20, CancellationToken ct = default);

	/// <summary>
	/// Freeze wallet
	/// </summary>
	Task<UnitResult<Error>> FreezeWalletAsync(Guid userId, CancellationToken ct = default);

	/// <summary>
	/// Unfreeze wallet
	/// </summary>
	Task<UnitResult<Error>> UnfreezeWalletAsync(Guid userId, CancellationToken ct = default);
}