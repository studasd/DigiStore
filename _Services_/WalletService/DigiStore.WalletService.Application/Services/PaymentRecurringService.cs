using CSharpFunctionalExtensions;
using DigiStore.Enums;
using DigiStore.SharedKernel;
using DigiStore.WalletService.Application.DTOs;
using DigiStore.WalletService.Application.Interfaces;
using DigiStore.WalletService.Application.Validators;
using DigiStore.WalletService.Domain;
using Microsoft.Extensions.Logging;

namespace DigiStore.WalletService.Application.Services;

/// <summary>
/// Сервис управления рекуррентными платежами (подписки)
/// </summary>
public class PaymentRecurringService : IPaymentRecurringService
{
    private readonly IPaymentService _paymentService;
    private readonly IWalletRepository _walletRepository;
	private readonly IPaymentRecurringRepository _paymentRecurringRepository;
    private readonly PaymentValidator _paymentValidator;
    private readonly ILogger<PaymentRecurringService> _logger;

	public PaymentRecurringService(
		IPaymentService paymentService,
		IWalletRepository walletRepository,
		IPaymentRecurringRepository paymentRecurringRepository,
		PaymentValidator paymentValidator,
		ILogger<PaymentRecurringService> logger)
	{
        _paymentService = paymentService;
        _walletRepository = walletRepository;
		_paymentRecurringRepository = paymentRecurringRepository;
        _paymentValidator = paymentValidator;
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
		CancellationToken token = default)
	{
		try
		{
			_logger.LogInformation(
				$"YooKassa: Создание рекуррентного платежа - " +
				$"WalletId: {walletId}, Amount: {amount}, Interval: {intervalDays}");

			// Валидировать сумму
			var validationResult = _paymentValidator.ValidateDepositAmount(amount);
			if (validationResult.IsFailure)
				return validationResult.Error;

			// Проверить кошелек
			var wallet = _walletRepository.GetByIdAsync(walletId, token);
			if (wallet == null)
				return Error.Failure("", "Кошелек не найден");

			// Создать подписку
			var recurring = PaymentRecurringDS.Create(walletId, userId, amount, intervalDays, description);

			// Добавить в БД
			var addResult = await _paymentRecurringRepository.AddAsync(recurring, token);
			if (addResult.IsFailure)
				return addResult.Error;

			_logger.LogInformation($"YooKassa: Рекуррентный платеж создан - RecurringPaymentId: {recurring.Id}");

			return recurring;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "YooKassa: Ошибка при создании рекуррентного платежа");
			return Error.Failure("error.internal", "Ошибка при создании рекуррентного платежа");
		}
	}


	/// <summary>
	/// Активировать подписку
	/// </summary>
	public async Task<UnitResult<Error>> ActivateRecurringPaymentAsync(Guid recurringPaymentId, CancellationToken token)
	{
		var recurringResult = await _paymentRecurringRepository.GetByIdAsync(recurringPaymentId, token);
		if (recurringResult.IsSuccess)
		{
			recurringResult.Value.Activate();
			return await _paymentRecurringRepository.UpdateAsync(recurringResult.Value, token);
		}

		return Result.Success<Error>();
	}

	/// <summary>
	/// Приостановить подписку
	/// </summary>
	public async Task<UnitResult<Error>> SuspendRecurringPaymentAsync(Guid recurringPaymentId, CancellationToken token)
	{
		var recurringResult = await _paymentRecurringRepository.GetByIdAsync(recurringPaymentId, token);
		if (recurringResult.IsSuccess)
		{
			recurringResult.Value.Suspend();
			return await _paymentRecurringRepository.UpdateAsync(recurringResult.Value, token);
		}

		return Result.Success<Error>();
	}

	/// <summary>
	/// Отменить подписку
	/// </summary>
	public async Task<UnitResult<Error>> CancelRecurringPaymentAsync(Guid recurringPaymentId, CancellationToken token)
	{
		var recurringResult = await _paymentRecurringRepository.GetByIdAsync(recurringPaymentId, token);
		if (recurringResult.IsSuccess)
		{
			recurringResult.Value.Cancel();
			return await _paymentRecurringRepository.UpdateAsync(recurringResult.Value, token);
		}

		return Result.Success<Error>();
	}

	/// <summary>
	/// Обработать следующий платеж подписки
	/// </summary>
	public async Task<UnitResult<Error>> ProcessNextRecurringPaymentAsync(Guid recurringPaymentId, CancellationToken token)
	{
		var recurringResult = await _paymentRecurringRepository.GetByIdAsync(recurringPaymentId, token);
		if (recurringResult.IsFailure || recurringResult.Value.IsTimeForNextPayment == false)
			return recurringResult.Error;

		_logger.LogInformation(
			$"YooKassa: Обработка рекуррентного платежа - RecurringPaymentId: {recurringPaymentId}");

		var recurring = recurringResult.Value;

		// Создать платеж
		var paymentResult = await _paymentService.CreatePaymentAsync(
			recurring.UserId,
			recurring.WalletId,
			recurring.Amount,
			PaymentAggregators.None,
			$"Рекуррентный платеж - {recurring.Description}", 
			"");

		if (recurringResult.IsFailure)
		{
			recurring.RecordFailedPayment();
			var updateResult = await _paymentRecurringRepository.UpdateAsync(recurringResult.Value, token);
			if (updateResult.IsFailure)
				return updateResult.Error;

			_logger.LogError($"YooKassa: Ошибка при обработке рекуррентного платежа - {recurringResult.Error.GetMessage()}");
			return recurringResult.Error;
		}

		var payment = paymentResult.Value;

		payment.RecurringPaymentId = recurringPaymentId;
		recurring.RecordSuccessfulPayment();

		var updateResult2 = await _paymentRecurringRepository.UpdateAsync(recurring, token);
		if (updateResult2.IsFailure)
			return updateResult2.Error;

		// Завершить платеж сразу для подписок
		await _paymentService.CompletePaymentAsync(new PaymentSuccessDTO(
			payment.AggregatorPaymentId.ToString(),
			payment.Amount,
			"RecurringPayment"
			), token);

		_logger.LogInformation($"YooKassa: Рекуррентный платеж обработан - PaymentId: {payment.Id}");

		return Result.Success<Error>();
	}

}