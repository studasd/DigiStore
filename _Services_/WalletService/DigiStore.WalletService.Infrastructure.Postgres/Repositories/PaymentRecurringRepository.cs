using CSharpFunctionalExtensions;
using DigiStore.SharedKernel;
using DigiStore.WalletService.Application.Interfaces;
using DigiStore.WalletService.Domain;
using DigiStore.WalletService.Infrastructure.Postgres.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DigiStore.WalletService.Infrastructure.Postgres.Repositories;

public class PaymentRecurringRepository : IPaymentRecurringRepository
{
    private readonly WalletDbContext _context;
    private readonly ILogger<PaymentRecurringRepository> _logger;

    public PaymentRecurringRepository(WalletDbContext context, ILogger<PaymentRecurringRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result<PaymentRecurringDS, Error>> AddAsync(PaymentRecurringDS recurring, CancellationToken ct = default)
    {
        try
        {
            _context.PaymentRecurrings.Add(recurring);
            await _context.SaveChangesAsync(ct);
            _logger.LogInformation("Recurring payment created: {RecurringId}", recurring.Id);
            return recurring;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to add recurring payment for user {UserId}", recurring.UserId);
            return Error.Failure("recurring.add_failed", ex.Message);
        }
    }

    public async Task<Result<PaymentRecurringDS, Error>> GetByIdAsync(Guid recurringId, CancellationToken ct = default)
    {
        var r = await _context.PaymentRecurrings
            .Include(rp => rp.Payments)
            .FirstOrDefaultAsync(p => p.Id == recurringId, ct);

        if (r == null)
            return Error.NotFound("recurring.not_found", "Recurring payment not found");

        return r;
    }

    public async Task<Result<List<PaymentRecurringDS>, Error>> GetDueAsync(CancellationToken ct = default)
    {
        try
        {
            var list = await _context.PaymentRecurrings
                .Where(r => r.IsActive && r.NextPaymentDate <= DateTime.UtcNow)
                .ToListAsync(ct);

			return list;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to query due recurring payments");
            return Error.Failure("recurring.query_failed", ex.Message);
        }
    }

    public async Task<Result<List<PaymentRecurringDS>, Error>> GetUserRecurringPaymentsAsync(Guid userId, int skip = 0, int take = 10, CancellationToken ct = default)
    {
        try
        {
            var list = await _context.PaymentRecurrings
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.CreatedAt)
                .Skip(skip)
                .Take(take)
                .ToListAsync(ct);

            return list;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get recurring payments for user {UserId}", userId);
            return Error.Failure("recurring.query_failed", ex.Message);
        }
    }

    public async Task<Result<PaymentRecurringDS, Error>> UpdateAsync(PaymentRecurringDS recurring, CancellationToken ct = default)
    {
        try
        {
            _context.PaymentRecurrings.Update(recurring);
            await _context.SaveChangesAsync(ct);
            _logger.LogInformation("Recurring payment updated: {RecurringId}", recurring.Id);
            return recurring;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update recurring payment {RecurringId}", recurring.Id);
            return Error.Failure("recurring.update_failed", ex.Message);
        }
    }
}
