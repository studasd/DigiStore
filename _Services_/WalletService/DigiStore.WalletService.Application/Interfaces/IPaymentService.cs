using CSharpFunctionalExtensions;
using DigiStore.Enums;
using DigiStore.SharedKernel;
using DigiStore.WalletService.Domain;

namespace DigiStore.WalletService.Application.Interfaces;

public interface IPaymentService
{
	/// Создать платеж
	Task<Result<PaymentDS, Error>> CreatePaymentAsync(Guid userId, Guid walletId, decimal amount, string description = "", CancellationToken ct = default);

	/// Получить платеж по ID
	Task<Result<PaymentDS, Error>> GetPaymentAsync(Guid paymentId, CancellationToken ct = default);

	/// Получить платеж по ID YooKassa
	Task<Result<PaymentDS, Error>> GetPaymentByYooKassaIdAsync(string yooKassaPaymentId, CancellationToken ct = default);

	/// Обновить статус платежа
	Task UpdatePaymentStatusAsync(Guid paymentId, PaymentStatus status, CancellationToken ct = default);

	/// Завершить платеж
	Task CompletePaymentAsync(Guid paymentId, CancellationToken ct = default);

	/// Отменить платеж
	Task CancelPaymentAsync(Guid paymentId, string? reason = null, CancellationToken ct = default);

	/// Получить платежи пользователя
	Task<Result<IReadOnlyList<PaymentDS>, Error>> GetUserPaymentsAsync(Guid userId, int skip = 0, int take = 10, CancellationToken ct = default);

	/// Получить ссылку на оплату
	Task<string?> GetPaymentConfirmationUrlAsync(Guid paymentId, CancellationToken ct = default);
}
