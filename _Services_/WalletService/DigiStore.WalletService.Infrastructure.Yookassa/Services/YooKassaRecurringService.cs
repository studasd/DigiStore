using DigiStore.WalletService.Application.Configurations;
using DigiStore.WalletService.Domain;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace DigiStore.WalletService.Infrastructure.Yookassa.Services;

/// <summary>
/// Сервис управления рекуррентными платежами (подписки)
/// </summary>
public class YooKassaRecurringService
{
	private readonly YooKassaPaymentService _paymentService;
	private readonly PaymentValidator _validator;
	private readonly WalletDbContext _dbContext;
	private readonly ILogger<YooKassaRecurringService> _logger;

	public YooKassaRecurringService(
		YooKassaPaymentService paymentService,
		PaymentValidator validator,
		WalletDbContext dbContext,
		ILogger<YooKassaRecurringService> logger)
	{
		_paymentService = paymentService;
		_validator = validator;
		_dbContext = dbContext;
		_logger = logger;
	}

	/// <summary>
	/// Создать новую подписку
	/// </summary>
	public async Task<(bool Success, PaymentRecurringDS? RecurringPayment, string? Error)> CreateRecurringPaymentAsync(
		Guid walletId,
		Guid userId,
		decimal amount,
		int intervalDays,
		string description = "")
	{
		try
		{
			_logger.LogInformation(
				$"YooKassa: Создание рекуррентного платежа - " +
				$"WalletId: {walletId}, Amount: {amount}, Interval: {intervalDays}");

			// Валидировать сумму
			var validation = _validator.ValidateDepositAmountOLD(amount);
			if (!validation.IsValid)
				return (false, null, validation.ErrorMessage);

			// Проверить кошелек
			var wallet = await _dbContext.Set<WalletDS>()
				.FirstOrDefaultAsync(w => w.Id == walletId);
			if (wallet == null)
				return (false, null, "Кошелек не найден");

			// Создать подписку
			var recurring = PaymentRecurringDS.Create(
				walletId, userId, amount, intervalDays, description);

			// Добавить в БД
			_dbContext.Set<PaymentRecurringDS>().Add(recurring);
			await _dbContext.SaveChangesAsync();

			_logger.LogInformation(
				$"YooKassa: Рекуррентный платеж создан - RecurringPaymentId: {recurring.Id}");

			return (true, recurring, null);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "YooKassa: Ошибка при создании рекуррентного платежа");
			return (false, null, "Внутренняя ошибка сервера");
		}
	}

	/// <summary>
	/// Получить подписку по ID
	/// </summary>
	public async Task<PaymentRecurringDS?> GetRecurringPaymentAsync(Guid recurringPaymentId)
	{
		return await _dbContext.Set<PaymentRecurringDS>()
			.Include(r => r.Payments)
			.FirstOrDefaultAsync(r => r.Id == recurringPaymentId);
	}

	/// <summary>
	/// Активировать подписку
	/// </summary>
	public async Task ActivateRecurringPaymentAsync(Guid recurringPaymentId)
	{
		var recurring = await GetRecurringPaymentAsync(recurringPaymentId);
		if (recurring != null)
		{
			recurring.Activate();
			await _dbContext.SaveChangesAsync();
		}
	}

	/// <summary>
	/// Приостановить подписку
	/// </summary>
	public async Task SuspendRecurringPaymentAsync(Guid recurringPaymentId)
	{
		var recurring = await GetRecurringPaymentAsync(recurringPaymentId);
		if (recurring != null)
		{
			recurring.Suspend();
			await _dbContext.SaveChangesAsync();
		}
	}

	/// <summary>
	/// Отменить подписку
	/// </summary>
	public async Task CancelRecurringPaymentAsync(Guid recurringPaymentId)
	{
		var recurring = await GetRecurringPaymentAsync(recurringPaymentId);
		if (recurring != null)
		{
			recurring.Cancel();
			await _dbContext.SaveChangesAsync();
		}
	}

	/// <summary>
	/// Обработать следующий платеж подписки
	/// </summary>
	public async Task ProcessNextRecurringPaymentAsync(Guid recurringPaymentId)
	{
		var recurring = await GetRecurringPaymentAsync(recurringPaymentId);
		if (recurring == null || !recurring.IsTimeForNextPayment)
			return;

		_logger.LogInformation(
			$"YooKassa: Обработка рекуррентного платежа - RecurringPaymentId: {recurringPaymentId}");

		// Создать платеж
		var (success, payment, error) = await _paymentService.CreatePaymentAsync(
			recurring.WalletId,
			recurring.UserId,
			recurring.Amount,
			$"Рекуррентный платеж - {recurring.Description}");

		if (success && payment != null)
		{
			payment.RecurringPaymentId = recurringPaymentId;
			recurring.RecordSuccessfulPayment();

			await _dbContext.SaveChangesAsync();

			// Завершить платеж сразу для подписок
			await _paymentService.CompletePaymentAsync(payment.Id);

			_logger.LogInformation(
				$"YooKassa: Рекуррентный платеж обработан - PaymentId: {payment.Id}");
		}
		else
		{
			recurring.RecordFailedPayment();
			await _dbContext.SaveChangesAsync();

			_logger.LogError(
				$"YooKassa: Ошибка при обработке рекуррентного платежа - {error}");
		}
	}

	/// <summary>
	/// Получить подписки, готовые к обработке
	/// </summary>
	public async Task<List<PaymentRecurringDS>> GetDueRecurringPaymentsAsync()
	{
		return await _dbContext.Set<PaymentRecurringDS>()
			.Where(r => r.IsActive && r.NextPaymentDate <= DateTime.UtcNow)
			.ToListAsync();
	}

	/// <summary>
	/// Получить подписки пользователя
	/// </summary>
	public async Task<List<PaymentRecurringDS>> GetUserRecurringPaymentsAsync(
		Guid userId,
		int skip = 0,
		int take = 10)
	{
		return await _dbContext.Set<PaymentRecurringDS>()
			.Where(r => r.UserId == userId)
			.OrderByDescending(r => r.CreatedAt)
			.Skip(skip)
			.Take(take)
			.ToListAsync();
	}
}