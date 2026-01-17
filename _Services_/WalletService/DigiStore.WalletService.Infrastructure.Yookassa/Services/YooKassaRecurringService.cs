using CSharpFunctionalExtensions;
using DigiStore.SharedKernel;
using DigiStore.WalletService.Application.Configurations;
using DigiStore.WalletService.Application.Interfaces;
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
    private readonly IWalletRepository _walletRepository;
    private readonly IPaymentRecurringRepository _paymentRecurringRepository;
    private readonly ILogger<YooKassaRecurringService> _logger;

	public YooKassaRecurringService(
		YooKassaPaymentService paymentService,
		PaymentValidator validator,
		IWalletRepository walletRepository,
		IPaymentRecurringRepository paymentRecurringRepository,
		ILogger<YooKassaRecurringService> logger)
	{
		_paymentService = paymentService;
		_validator = validator;
        _walletRepository = walletRepository;
        _paymentRecurringRepository = paymentRecurringRepository;
        _logger = logger;
	}

	/// <summary>
	/// Создать новую подписку
	/// </summary>
	public async Task<Result<PaymentRecurringDS, Error>> CreateRecurringPaymentAsync(
		Guid walletId,
		Guid userId,
		decimal amount,
		int intervalDays,
		string description = "",
		CancellationToken ct = default)
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
			var wallet = _walletRepository.GetByIdAsync(walletId, ct);
			if (wallet == null)
				return Error.Failure("", "Кошелек не найден");

			// Создать подписку
			var recurring = PaymentRecurringDS.Create(walletId, userId, amount, intervalDays, description);

			// Добавить в БД
			await _paymentRecurringRepository.AddAsync(recurring, ct);

			_logger.LogInformation(
				$"YooKassa: Рекуррентный платеж создан - RecurringPaymentId: {recurring.Id}");

			return recurring;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "YooKassa: Ошибка при создании рекуррентного платежа");
			return Error.Failure("error.internal", "Ошибка при создании рекуррентного платежа");
		}
	}

	/// <summary>
	/// Получить подписку по ID
	/// </summary>
	public async Task<Result<PaymentRecurringDS, Error>> GetRecurringPaymentAsync(Guid recurringPaymentId, CancellationToken ct)
	{
		return await _paymentRecurringRepository.GetByIdAsync(recurringPaymentId, ct);
	}

	/// <summary>
	/// Активировать подписку
	/// </summary>
	public async Task ActivateRecurringPaymentAsync(Guid recurringPaymentId, CancellationToken ct)
	{
		var recurringResult = await GetRecurringPaymentAsync(recurringPaymentId, ct);
		if (recurringResult.IsSuccess)
		{
			recurringResult.Value.Activate();
			await _paymentRecurringRepository.UpdateAsync(recurringResult.Value, ct);
		}
	}

	/// <summary>
	/// Приостановить подписку
	/// </summary>
	public async Task SuspendRecurringPaymentAsync(Guid recurringPaymentId, CancellationToken ct)
	{
		var recurringResult = await GetRecurringPaymentAsync(recurringPaymentId, ct	);
		if (recurringResult.IsSuccess)
		{
			recurringResult.Value.Suspend();
			await _paymentRecurringRepository.UpdateAsync(recurringResult.Value, ct);
		}
	}

	/// <summary>
	/// Отменить подписку
	/// </summary>
	public async Task CancelRecurringPaymentAsync(Guid recurringPaymentId, CancellationToken ct)
	{
		var recurringResult = await GetRecurringPaymentAsync(recurringPaymentId, ct);
		if (recurringResult.IsSuccess)
		{
			recurringResult.Value.Cancel();
			await _paymentRecurringRepository.UpdateAsync(recurringResult.Value);
		}
	}

	/// <summary>
	/// Обработать следующий платеж подписки
	/// </summary>
	public async Task ProcessNextRecurringPaymentAsync(Guid recurringPaymentId, CancellationToken ct)
	{
		var recurringResult = await GetRecurringPaymentAsync(recurringPaymentId, ct);
		if (recurringResult.IsFailure || recurringResult.Value.IsTimeForNextPayment == false)
			return;

		_logger.LogInformation(
			$"YooKassa: Обработка рекуррентного платежа - RecurringPaymentId: {recurringPaymentId}");

		var recurring = recurringResult.Value;

		// Создать платеж
		var paymentResult = await _paymentService.CreatePaymentAsync(
			recurring.WalletId,
			recurring.UserId,
			recurring.Amount,
			$"Рекуррентный платеж - {recurring.Description}");

		if (recurringResult.IsFailure)
		{
			recurring.RecordFailedPayment();
			await _paymentRecurringRepository.UpdateAsync(recurringResult.Value, ct);

			_logger.LogError($"YooKassa: Ошибка при обработке рекуррентного платежа - {recurringResult.Error.GetMessage()}");
			return;
		}

		var payment = paymentResult.Value;

		payment.RecurringPaymentId = recurringPaymentId;
		recurring.RecordSuccessfulPayment();

		await _paymentRecurringRepository.UpdateAsync(recurring, ct);

		// Завершить платеж сразу для подписок
		await _paymentService.CompletePaymentAsync(payment.Id);

		_logger.LogInformation($"YooKassa: Рекуррентный платеж обработан - PaymentId: {payment.Id}");
	}

	/// <summary>
	/// Получить подписки, готовые к обработке
	/// </summary>
	public async Task<Result<List<PaymentRecurringDS>, Error>> GetDueRecurringPaymentsAsync(CancellationToken ct)
	{
		return await _paymentRecurringRepository.GetDueAsync(ct);
	}

	/// <summary>
	/// Получить подписки пользователя
	/// </summary>
	public async Task<Result<List<PaymentRecurringDS>, Error>> GetUserRecurringPaymentsAsync(
		Guid userId,
		int skip = 0,
		int take = 10,
		CancellationToken ct = default)
	{
		return await _paymentRecurringRepository.GetUserRecurringPaymentsAsync(userId, skip, take, ct);
	}
}