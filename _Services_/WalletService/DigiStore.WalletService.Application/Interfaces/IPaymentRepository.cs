using CSharpFunctionalExtensions;
using DigiStore.SharedKernel;
using DigiStore.WalletService.Domain;

namespace DigiStore.WalletService.Application.Interfaces;

public interface IPaymentRepository
{
    Task<Result<PaymentDS, Error>> AddAsync(PaymentDS payment, CancellationToken ct = default);

    Task<Result<PaymentDS, Error>> GetByIdAsync(Guid paymentId, CancellationToken ct = default);

    Task<Result<PaymentDS, Error>> GetByAggregatorIdAsync(string aggregatorPaymentId, CancellationToken ct = default);

    Task<Result<List<PaymentDS>, Error>> GetUserPaymentsAsync(Guid userId, int skip = 0, int take = 10, CancellationToken ct = default);

    Task<Result<PaymentDS, Error>> UpdateAsync(PaymentDS payment, CancellationToken ct = default);
}
