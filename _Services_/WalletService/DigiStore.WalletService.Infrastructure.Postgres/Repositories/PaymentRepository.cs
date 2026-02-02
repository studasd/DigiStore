using CSharpFunctionalExtensions;
using DigiStore.Enums;
using StudCoreKit.SharedKernel;
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


    public async Task<Result<PaymentDS, Error>> AddAsync(PaymentDS payment, CancellationToken token)
    {
        _context.Payments.Add(payment);

		var saveResult = await SaveChangesAsync(token);
		if (saveResult.IsFailure)
			return saveResult.Error;

		_logger.LogInformation("Payment created: {PaymentId}", payment.Id);
        return payment;
    }

    public async Task<Result<PaymentDS, Error>> GetByIdAsync(Guid paymentId, CancellationToken token)
    {
        var payment = await _context.Payments
            .FirstOrDefaultAsync(p => p.Id == paymentId, token);

        if (payment == null)
            return Error.NotFound("payment.not_found", "Payment not found");

        return payment;
    }

	public async Task<Result<PaymentDS, Error>> GetByAggregatorIdForUpdateAsync(string aggregatorPaymentId, CancellationToken token)
	{
        FormattableString sql = $@"SELECT * FROM ""WalletService"".""Payments"" WHERE ""AggregatorPaymentId"" = {aggregatorPaymentId} FOR UPDATE";

		var payment = await _context.Payments
			.FromSqlInterpolated(sql)
            .AsTracking()
            .FirstOrDefaultAsync(token);

        if (payment == null)
            return Error.NotFound("payment.not_found", "Payment not found");

        return payment;
	}

    public async Task<Result<PaymentDS, Error>> GetByAggregatorIdAsync(string aggregatorPaymentId, CancellationToken token)
    {
        var payment = await _context.Payments
            .FirstOrDefaultAsync(p => p.AggregatorPaymentId == aggregatorPaymentId, token);

        if (payment == null)
            return Error.NotFound("payment.not_found", "Payment not found");

        return payment;
    }

    public async Task<Result<IReadOnlyList<PaymentDS>, Error>> GetUserPaymentsAsync(Guid userId, int skip = 0, int take = 10, CancellationToken token = default)
    {
        try
        {
			var list = await _context.Payments
                .Where(p => p.UserId == userId)
                .OrderByDescending(p => p.CreatedAt)
                .Skip(skip)
                .Take(take)
                .ToListAsync(token);

            return list;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get payments for user {UserId}", userId);
            return Error.Failure("payment.query_failed", ex.Message);
        }
    }

    public async Task<Result<PaymentDS, Error>> UpdateAsync(PaymentDS payment, CancellationToken token)
    {
        _context.Payments.Update(payment);

		var saveResult = await SaveChangesAsync(token);
		if (saveResult.IsFailure)
			return saveResult.Error;

		_logger.LogInformation("Payment updated: {PaymentId}", payment.Id);
        return payment;
    }


	/// <summary>
	/// Обновить статус платежа
	/// </summary>
	public async Task<UnitResult<Error>> UpdatePaymentStatusAsync(Guid paymentId, PaymentStatus status, CancellationToken token)
	{
		var paymentResult = await GetByIdAsync(paymentId, token);

        if(paymentResult.IsFailure)
            return paymentResult.Error;

		var payment = paymentResult.Value;
		payment.Status = status;
		payment.UpdatedAt = DateTime.UtcNow;
		
        return await UpdateAsync(payment, token);
	}


	/// <summary>
	/// Отменить платеж
	/// </summary>
	public async Task<UnitResult<Error>> CancelPaymentAsync(Guid paymentId, string? errorMessage = null, string? paymentMethod = null, CancellationToken token = default)
	{
		var paymentResult = await GetByIdAsync(paymentId, token);
		if (paymentResult.IsFailure)
			return paymentResult.Error;

		var payment = paymentResult.Value;

		payment.MarkAsCanceled(errorMessage, paymentMethod);

		var updateResult = await UpdateAsync(payment, token);
        if (updateResult.IsFailure)
            return updateResult.Error;

        _logger.LogInformation($"YooKassa: Платеж отменен - PaymentId: {paymentId}");
        return Result.Success<Error>();
	}


	public async Task<UnitResult<Error>> SaveChangesAsync(CancellationToken token)
	{
		try
		{
			await _context.SaveChangesAsync(token);
		}
		catch (DbUpdateException ex)
		{
			_logger.LogWarning(ex, "Failed save changes");

			return Error.Failure("failed.db.savechange", $"Failed save changes");
		}

		return Result.Success<Error>();
	}
}
