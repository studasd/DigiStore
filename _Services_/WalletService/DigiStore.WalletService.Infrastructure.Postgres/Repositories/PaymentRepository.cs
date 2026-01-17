using CSharpFunctionalExtensions;
using DigiStore.SharedKernel;
using DigiStore.WalletService.Application.Interfaces;
using DigiStore.WalletService.Domain;
using DigiStore.WalletService.Infrastructure.Postgres.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DigiStore.WalletService.Infrastructure.Postgres.Repositories;

public class PaymentRepository : IPaymentRepository
{
    private readonly WalletDbContext _context;
    private readonly ILogger<PaymentRepository> _logger;

    public PaymentRepository(WalletDbContext context, ILogger<PaymentRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result<PaymentDS, Error>> AddAsync(PaymentDS payment, CancellationToken ct = default)
    {
        try
        {
            _context.YooKassaPayments.Add(payment);
            await _context.SaveChangesAsync(ct);
            _logger.LogInformation("Payment created: {PaymentId}", payment.Id);
            return payment;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to add payment for user {UserId}", payment.UserId);
            return Error.Failure("payment.add_failed", ex.Message);
        }
    }

    public async Task<Result<PaymentDS, Error>> GetByIdAsync(Guid paymentId, CancellationToken ct = default)
    {
        var payment = await _context.YooKassaPayments
            .FirstOrDefaultAsync(p => p.Id == paymentId, ct);

        if (payment == null)
            return Error.NotFound("payment.not_found", "Payment not found");

        return payment;
    }

    public async Task<Result<PaymentDS, Error>> GetByAggregatorIdAsync(string aggregatorPaymentId, CancellationToken ct = default)
    {
        var payment = await _context.YooKassaPayments
            .FirstOrDefaultAsync(p => p.AggregatorPaymentId == aggregatorPaymentId, ct);

        if (payment == null)
            return Error.NotFound("payment.not_found", "Payment not found");

        return payment;
    }

    public async Task<Result<List<PaymentDS>, Error>> GetUserPaymentsAsync(Guid userId, int skip = 0, int take = 10, CancellationToken ct = default)
    {
        try
        {
            var list = await _context.YooKassaPayments
                .Where(p => p.UserId == userId)
                .OrderByDescending(p => p.CreatedAt)
                .Skip(skip)
                .Take(take)
                .ToListAsync(ct);

            return list;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get payments for user {UserId}", userId);
            return Error.Failure("payment.query_failed", ex.Message);
        }
    }

    public async Task<Result<PaymentDS, Error>> UpdateAsync(PaymentDS payment, CancellationToken ct = default)
    {
        try
        {
            _context.YooKassaPayments.Update(payment);
            await _context.SaveChangesAsync(ct);
            _logger.LogInformation("Payment updated: {PaymentId}", payment.Id);
            return payment;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update payment {PaymentId}", payment.Id);
            return Error.Failure("payment.update_failed", ex.Message);
        }
    }
}
