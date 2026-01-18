using CSharpFunctionalExtensions;
using DigiStore.Enums;
using DigiStore.SharedKernel;
using DigiStore.TgBot.Application.DTOs;

namespace DigiStore.TgBot.Application.Interfaces.Services;


/// <summary>
/// Service to interact with WalletService via HTTP
/// </summary>
public interface IWalletService
{
	/// <summary>
	/// Get user balance
	/// </summary>
	Task<Result<BalanceDto, Error>> GetBalanceAsync(Guid userId, CancellationToken token);

	/// <summary>
	/// Get transactions
	/// </summary>
	Task<Result<IEnumerable<TransactionDto>, Error>> GetTransactionsAsync(Guid userId, int take = 10, CancellationToken token = default);

	/// <summary>
	/// Initiate withdrawal
	/// </summary>
	Task<Result<bool, Error>> InitiateWithdrawalAsync(Guid userId, decimal amount, CancellationToken token);

	Task<Result<string, Error>> CreatePaymentAsync(Guid userId, PaymentAggregators paymentAggregator, decimal amount, CancellationToken token);
}
