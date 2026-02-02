using CSharpFunctionalExtensions;
using StudCoreKit.SharedKernel;

namespace DigiStore.WalletService.Application.Interfaces;

public interface IYookassaProvider
{
	Task<Result<string, Error>> CreatePaymentAsync(
		Guid userId, 
		Guid walletId, 
		Guid paymentId, 
		decimal amount, 
		string description, 
		string returnUrl, 
		CancellationToken token = default);

	Task<Result<string, Error>> GetPaymentConfirmationUrlAsync(string aggregatorPaymentId, CancellationToken token);

	Task<Result<string, Error>> CreateWithdrawalAsync(
		Guid walletId,
		Guid withdrawalId,
		decimal amount,
		decimal actualAmount,
		CancellationToken token);

	Task<Result<string, Error>> CapturePaymentAsync(string paymentId, CancellationToken token);
}
