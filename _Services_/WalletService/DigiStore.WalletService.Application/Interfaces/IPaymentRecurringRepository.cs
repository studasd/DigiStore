using CSharpFunctionalExtensions;
using DigiStore.SharedKernel;
using DigiStore.WalletService.Domain;

namespace DigiStore.WalletService.Application.Interfaces;

public interface IPaymentRecurringRepository
{
    Task<Result<PaymentRecurringDS, Error>> AddAsync(PaymentRecurringDS recurring, CancellationToken token);

    Task<Result<PaymentRecurringDS, Error>> GetByIdAsync(Guid recurringId, CancellationToken token);

    Task<Result<List<PaymentRecurringDS>, Error>> GetDueAsync(CancellationToken token);

    Task<Result<List<PaymentRecurringDS>, Error>> GetUserRecurringPaymentsAsync(Guid userId, int skip = 0, int take = 10, CancellationToken token = default);

	Task<UnitResult<Error>> UpdateAsync(PaymentRecurringDS recurring, CancellationToken token);
}
