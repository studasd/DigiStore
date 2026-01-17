using CSharpFunctionalExtensions;
using DigiStore.Enums;
using DigiStore.SharedKernel;
using DigiStore.WalletService.Domain;

namespace DigiStore.WalletService.Application.Interfaces;

public interface IPaymentService
{
	/// Создать платеж
	Task<Result<PaymentDS, Error>> CreatePaymentAsync(Guid userId, Guid walletId, decimal amount, string description = "");

	/// Получить платеж по ID
	Task<Result<PaymentDS, Error>> GetPaymentAsync(Guid paymentId);

	/// Получить платеж по ID YooKassa
	Task<Result<PaymentDS, Error>> GetPaymentByYooKassaIdAsync(string yooKassaPaymentId);

	/// Обновить статус платежа
	Task UpdatePaymentStatusAsync(Guid paymentId, PaymentStatus status);

	/// Завершить платеж
	Task CompletePaymentAsync(Guid paymentId);

	/// Отменить платеж
	Task CancelPaymentAsync(Guid paymentId, string? reason = null);

	/// Получить платежи пользователя
	Task<List<PaymentDS>> GetUserPaymentsAsync(Guid userId, int skip = 0, int take = 10);

	/// Получить ссылку на оплату
	Task<string?> GetPaymentConfirmationUrlAsync(Guid paymentId);
}
