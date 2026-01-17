using CSharpFunctionalExtensions;
using DigiStore.SharedKernel;
using DigiStore.WalletService.Application.Interfaces;
using DigiStore.WalletService.Application.Validators;
using DigiStore.WalletService.Domain;
using Microsoft.Extensions.Logging;

namespace DigiStore.WalletService.Application.Services;

/// <summary>
/// Сервис управления рекуррентными платежами (подписки)
/// </summary>
public class PaymentRecurringService
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
		CancellationToken ct = default)
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
	/// Активировать подписку
	/// </summary>
	public async Task ActivateRecurringPaymentAsync(Guid recurringPaymentId, CancellationToken ct)
	{
		var recurringResult = await _paymentRecurringRepository.GetByIdAsync(recurringPaymentId, ct);
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
		var recurringResult = await _paymentRecurringRepository.GetByIdAsync(recurringPaymentId, ct);
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
		var recurringResult = await _paymentRecurringRepository.GetByIdAsync(recurringPaymentId, ct);
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
		var recurringResult = await _paymentRecurringRepository.GetByIdAsync(recurringPaymentId, ct);
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

}