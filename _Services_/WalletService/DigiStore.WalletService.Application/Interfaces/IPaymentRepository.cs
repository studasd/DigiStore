using CSharpFunctionalExtensions;
using DigiStore.Enums;
using DigiStore.SharedKernel;
using DigiStore.WalletService.Domain;

namespace DigiStore.WalletService.Application.Interfaces;

public interface IPaymentRepository
{
	Task<Result<PaymentDS, Error>> AddAsync(PaymentDS payment, CancellationToken ct);

	Task<Result<PaymentDS, Error>> GetByIdAsync(Guid paymentId, CancellationToken ct);

	Task<Result<PaymentDS, Error>> GetByAggregatorIdAsync(string aggregatorPaymentId, CancellationToken ct);

	Task<Result<IReadOnlyList<PaymentDS>, Error>> GetUserPaymentsAsync(Guid userId, int skip = 0, int take = 10, CancellationToken ct = default);

	Task<Result<PaymentDS, Error>> UpdateAsync(PaymentDS payment, CancellationToken ct);

	/// <summary>
	/// Обновить статус платежа
	/// </summary>
	Task<UnitResult<Error>> UpdatePaymentStatusAsync(Guid paymentId, PaymentStatus status, CancellationToken ct);

	/// <summary>
	/// Отменить платеж
	/// </summary>
	Task<UnitResult<Error>> CancelPaymentAsync(Guid paymentId, string? reason = null, CancellationToken ct = default);

	Task<UnitResult<Error>> SaveChangesAsync(CancellationToken ct);
}