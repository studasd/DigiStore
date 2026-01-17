using CSharpFunctionalExtensions;
using DigiStore.Enums;
using DigiStore.SharedKernel;
using DigiStore.WalletService.Domain;

namespace DigiStore.WalletService.Application.Interfaces;

public interface IPaymentRepository
{
	Task<Result<PaymentDS, Error>> AddAsync(PaymentDS payment, CancellationToken token);

	Task<Result<PaymentDS, Error>> GetByIdAsync(Guid paymentId, CancellationToken token);

	Task<Result<PaymentDS, Error>> GetByAggregatorIdAsync(string aggregatorPaymentId, CancellationToken token);

	Task<Result<IReadOnlyList<PaymentDS>, Error>> GetUserPaymentsAsync(Guid userId, int skip = 0, int take = 10, CancellationToken token = default);

	Task<Result<PaymentDS, Error>> UpdateAsync(PaymentDS payment, CancellationToken token);

	/// <summary>
	/// Обновить статус платежа
	/// </summary>
	Task<UnitResult<Error>> UpdatePaymentStatusAsync(Guid paymentId, PaymentStatus status, CancellationToken token);

	/// <summary>
	/// Отменить платеж
	/// </summary>
	Task<UnitResult<Error>> CancelPaymentAsync(Guid paymentId, string? reason = null, CancellationToken token = default);

	Task<UnitResult<Error>> SaveChangesAsync(CancellationToken token);
}