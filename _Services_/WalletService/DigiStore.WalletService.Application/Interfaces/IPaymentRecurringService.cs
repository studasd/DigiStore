using CSharpFunctionalExtensions;
using DigiStore.WalletService.Domain;
using StudCoreKit.SharedKernel;

namespace DigiStore.WalletService.Application.Interfaces;

public interface IPaymentRecurringService
{
	/// <summary>
	/// Создать новую подписку
	/// </summary>
	Task<Result<PaymentRecurringDS, Error>> CreateRecurringPaymentAsync(
		Guid walletId,
		Guid userId,
		decimal amount,
		int intervalDays,
		string description = "",
		CancellationToken token = default);

	/// <summary>
	/// Активировать подписку
	/// </summary>
	Task<UnitResult<Error>> ActivateRecurringPaymentAsync(Guid recurringPaymentId, CancellationToken token);

	/// <summary>
	/// Приостановить подписку
	/// </summary>
	Task<UnitResult<Error>> SuspendRecurringPaymentAsync(Guid recurringPaymentId, CancellationToken token);

	/// <summary>
	/// Отменить подписку
	/// </summary>
	Task<UnitResult<Error>> CancelRecurringPaymentAsync(Guid recurringPaymentId, CancellationToken token);

	/// <summary>
	/// Обработать следующий платеж подписки
	/// </summary>
	Task<UnitResult<Error>> ProcessNextRecurringPaymentAsync(Guid recurringPaymentId, CancellationToken token);
}
