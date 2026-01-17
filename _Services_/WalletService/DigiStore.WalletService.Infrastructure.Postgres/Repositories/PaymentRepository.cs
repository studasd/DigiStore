using CSharpFunctionalExtensions;
using DigiStore.Enums;
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


    public async Task<Result<PaymentDS, Error>> AddAsync(PaymentDS payment, CancellationToken ct)
    {
        _context.Payments.Add(payment);

		var saveResult = await SaveChangesAsync(ct);
		if (saveResult.IsFailure)
			return saveResult.Error;

		_logger.LogInformation("Payment created: {PaymentId}", payment.Id);
        return payment;
    }

    public async Task<Result<PaymentDS, Error>> GetByIdAsync(Guid paymentId, CancellationToken ct)
    {
        var payment = await _context.Payments
            .FirstOrDefaultAsync(p => p.Id == paymentId, ct);

        if (payment == null)
            return Error.NotFound("payment.not_found", "Payment not found");

        return payment;
    }

    public async Task<Result<PaymentDS, Error>> GetByAggregatorIdAsync(string aggregatorPaymentId, CancellationToken ct)
    {
        var payment = await _context.Payments
            .FirstOrDefaultAsync(p => p.AggregatorPaymentId == aggregatorPaymentId, ct);

        if (payment == null)
            return Error.NotFound("payment.not_found", "Payment not found");

        return payment;
    }

    public async Task<Result<IReadOnlyList<PaymentDS>, Error>> GetUserPaymentsAsync(Guid userId, int skip = 0, int take = 10, CancellationToken ct = default)
    {
        try
        {
			var list = await _context.Payments
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

    public async Task<Result<PaymentDS, Error>> UpdateAsync(PaymentDS payment, CancellationToken ct)
    {
        _context.Payments.Update(payment);

		var saveResult = await SaveChangesAsync(ct);
		if (saveResult.IsFailure)
			return saveResult.Error;

		_logger.LogInformation("Payment updated: {PaymentId}", payment.Id);
        return payment;
    }


	/// <summary>
	/// Обновить статус платежа
	/// </summary>
	public async Task<UnitResult<Error>> UpdatePaymentStatusAsync(Guid paymentId, PaymentStatus status, CancellationToken ct)
	{
		var paymentResult = await GetByIdAsync(paymentId, ct);

        if(paymentResult.IsFailure)
            return paymentResult.Error;

		var payment = paymentResult.Value;
		payment.Status = status;
		payment.UpdatedAt = DateTime.UtcNow;
		
        return await UpdateAsync(payment, ct);
	}


	/// <summary>
	/// Отменить платеж
	/// </summary>
	public async Task<UnitResult<Error>> CancelPaymentAsync(Guid paymentId, string? reason = null, CancellationToken ct = default)
	{
		var paymentResult = await GetByIdAsync(paymentId, ct);
		if (paymentResult.IsFailure)
			return paymentResult.Error;

		var payment = paymentResult.Value;

		payment.MarkAsCanceled(reason);

		var updateResult = await UpdateAsync(payment, ct);
        if (updateResult.IsFailure)
            return updateResult.Error;

        _logger.LogInformation($"YooKassa: Платеж отменен - PaymentId: {paymentId}");
        return Result.Success<Error>();
	}


	public async Task<UnitResult<Error>> SaveChangesAsync(CancellationToken ct)
	{
		try
		{
			await _context.SaveChangesAsync(ct);
		}
		catch (DbUpdateException ex)
		{
			_logger.LogWarning(ex, "Failed save changes");

			return Error.Failure("failed.db.savechange", $"Failed save changes");
		}

		return Result.Success<Error>();
	}
}
