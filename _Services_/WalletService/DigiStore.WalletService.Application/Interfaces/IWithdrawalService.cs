using CSharpFunctionalExtensions;
using StudCoreKit.SharedKernel;
using DigiStore.WalletService.Domain;

namespace DigiStore.WalletService.Application.Interfaces;

public interface IWithdrawalService
{
	Task<Result<WithdrawalDS, Error>> CreateWithdrawalAsync(
		Guid walletId,
		Guid userId,
		decimal amount,
		string cardNumber,
		CancellationToken token);

	/// <summary>
	/// Отменить выплату и вернуть средства
	/// </summary>
	Task<UnitResult<Error>> CancelWithdrawalAsync(Guid withdrawalId, string? reason = null, CancellationToken token = default);
}