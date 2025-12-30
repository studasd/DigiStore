using CSharpFunctionalExtensions;
using DigiStore.SharedKernel;
using DigiStore.TgBot.Application.DTOs;

namespace DigiStore.TgBot.Application.Interfaces;


/// <summary>
/// Service to interact with WalletService via HTTP
/// </summary>
public interface ITelegramWalletService
{
	/// <summary>
	/// Get user balance
	/// </summary>
	Task<Result<TelegramBalanceDto, Error>> GetBalanceAsync(Guid userId, CancellationToken ct = default);

	/// <summary>
	/// Get transactions
	/// </summary>
	Task<Result<IEnumerable<TelegramTransactionDto>, Error>> GetTransactionsAsync(Guid userId, int take = 10, CancellationToken ct = default);

	/// <summary>
	/// Initiate withdrawal
	/// </summary>
	Task<Result<bool, Error>> InitiateWithdrawalAsync(Guid userId, decimal amount, CancellationToken ct = default);
}
