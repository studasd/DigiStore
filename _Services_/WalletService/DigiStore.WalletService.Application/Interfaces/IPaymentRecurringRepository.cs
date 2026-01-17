using CSharpFunctionalExtensions;
using DigiStore.SharedKernel;
using DigiStore.WalletService.Domain;

namespace DigiStore.WalletService.Application.Interfaces;

public interface IPaymentRecurringRepository
{
    Task<Result<PaymentRecurringDS, Error>> AddAsync(PaymentRecurringDS recurring, CancellationToken ct = default);

    Task<Result<PaymentRecurringDS, Error>> GetByIdAsync(Guid recurringId, CancellationToken ct = default);

    Task<Result<List<PaymentRecurringDS>, Error>> GetDueAsync(CancellationToken ct = default);

    Task<Result<List<PaymentRecurringDS>, Error>> GetUserRecurringPaymentsAsync(Guid userId, int skip = 0, int take = 10, CancellationToken ct = default);

    Task<Result<PaymentRecurringDS, Error>> UpdateAsync(PaymentRecurringDS recurring, CancellationToken ct = default);
}
