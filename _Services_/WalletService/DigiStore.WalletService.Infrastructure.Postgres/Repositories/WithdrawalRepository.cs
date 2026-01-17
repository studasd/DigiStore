using CSharpFunctionalExtensions;
using DigiStore.SharedKernel;
using DigiStore.WalletService.Application.Interfaces;
using DigiStore.WalletService.Domain;
using DigiStore.WalletService.Domain.Enums;
using DigiStore.WalletService.Infrastructure.Postgres.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DigiStore.WalletService.Infrastructure.Postgres.Repositories;

public class WithdrawalRepository : IWithdrawalRepository
{
    private readonly WalletDbContext _context;
    private readonly ILogger<WithdrawalRepository> _logger;

    public WithdrawalRepository(WalletDbContext context, ILogger<WithdrawalRepository> logger)
    {
        _context = context;
        _logger = logger;
    }


    public async Task<Result<WithdrawalDS, Error>> AddAsync(WithdrawalDS withdrawal, CancellationToken ct)
    {
        _context.Withdrawals.Add(withdrawal);

		var saveResult = await SaveChangesAsync(ct);
		if (saveResult.IsFailure)
			return saveResult.Error;

		_logger.LogInformation("Withdrawal created: {WithdrawalId}", withdrawal.Id);
        return withdrawal;
    }

    public async Task<Result<WithdrawalDS, Error>> GetByIdAsync(Guid withdrawalId, CancellationToken ct)
    {
        var w = await _context.Withdrawals
            .FirstOrDefaultAsync(p => p.Id == withdrawalId, ct);

        if (w == null)
            return Error.NotFound("withdrawal.not_found", "Withdrawal not found");

		return w;
    }

    public async Task<Result<WithdrawalDS, Error>> GetByAggregatorIdAsync(string aggregatorWithdrawalId, CancellationToken ct)
    {
        var w = await _context.Withdrawals
            .FirstOrDefaultAsync(p => p.AggregatorWithdrawalId == aggregatorWithdrawalId, ct);

        if (w == null)
            return Error.NotFound("withdrawal.not_found", "Withdrawal not found");

        return w;
    }

    public async Task<Result<List<WithdrawalDS>, Error>> GetUserWithdrawalsAsync(Guid userId, int skip = 0, int take = 10, CancellationToken ct = default)
    {
        try
        {
            var list = await _context.Withdrawals
                .Where(p => p.UserId == userId)
                .OrderByDescending(p => p.CreatedAt)
                .Skip(skip)
                .Take(take)
                .ToListAsync(ct);

			return list;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get withdrawals for user {UserId}", userId);
            return Error.Failure("withdrawal.query_failed", ex.Message);
        }
    }

    public async Task<UnitResult<Error>> UpdateAsync(WithdrawalDS withdrawal, CancellationToken ct)
    {
        _context.Withdrawals.Update(withdrawal);

		var saveResult = await SaveChangesAsync(ct);
		if (saveResult.IsFailure)
			return saveResult.Error;

		_logger.LogInformation("Withdrawal updated: {WithdrawalId}", withdrawal.Id);
        
        return Result.Success<Error>() ;
    }



	/// <summary>
	/// Обновить статус выплаты
	/// </summary>
	public async Task<UnitResult<Error>> UpdateWithdrawalStatusAsync(Guid withdrawalId, WithdrawalStatus status, CancellationToken ct)
	{
		var withdrawalResult = await GetByIdAsync(withdrawalId, ct);
		if (withdrawalResult.IsFailure)
            return withdrawalResult.Error;

		var withdrawal = withdrawalResult.Value;
		withdrawal.Status = status;
		withdrawal.UpdatedAt = DateTime.UtcNow;

		if (status == WithdrawalStatus.Succeeded)
		{
			withdrawal.MarkAsSucceeded();
		}

		return await UpdateAsync(withdrawal, ct);
	}


	/// <summary>
	/// Завершить выплату
	/// </summary>
	public async Task<UnitResult<Error>> CompleteWithdrawalAsync(Guid withdrawalId, CancellationToken ct)
	{
		var withdrawalResult = await GetByIdAsync(withdrawalId, ct);
		if (withdrawalResult.IsFailure)
			return withdrawalResult.Error;

		var withdrawal = withdrawalResult.Value;
		withdrawal.MarkAsSucceeded();
		
		return await UpdateAsync(withdrawal, ct);

		_logger.LogInformation(
			$"YooKassa: Выплата завершена - WithdrawalId: {withdrawalId}, " +
			$"Amount: {withdrawal.ActualAmount}");
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
