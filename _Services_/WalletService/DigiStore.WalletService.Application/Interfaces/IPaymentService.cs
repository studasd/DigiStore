using CSharpFunctionalExtensions;
using DigiStore.Enums;
using DigiStore.SharedKernel;
using DigiStore.WalletService.Domain;

namespace DigiStore.WalletService.Application.Interfaces;

public interface IPaymentService
{
	/// Создать платеж
	Task<Result<PaymentDS, Error>> CreatePaymentAsync(
		Guid userId, 
		Guid walletId, 
		decimal amount, 
		PaymentAggregators aggregator, 
		string description = "", 
		string username = "", 
		CancellationToken token = default);

	/// Завершить платеж
	Task<UnitResult<Error>> CompletePaymentAsync(Guid paymentId, CancellationToken token);

	/// Получить ссылку на оплату
	Task<Result<string, Error>> GetPaymentConfirmationUrlAsync(Guid paymentId, CancellationToken token);
}
