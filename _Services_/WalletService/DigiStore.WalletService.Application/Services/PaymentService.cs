using CSharpFunctionalExtensions;
using DigiStore.Enums;
using DigiStore.SharedKernel;
using DigiStore.WalletService.Application.Interfaces;
using DigiStore.WalletService.Domain;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace DigiStore.WalletService.Application.Services;


/// <summary>
/// Сервис управления платежами
/// </summary>
public class PaymentService : IPaymentService
{
	private readonly IPaymentRepository _paymentRepository;
	private readonly IWalletRepository _walletRepository;
	private readonly ILogger<PaymentService> _logger;

	public PaymentService(
		IPaymentRepository paymentRepository,
		IWalletRepository walletRepository,
		ILogger<PaymentService> logger)
	{
		_paymentRepository = paymentRepository;
		_walletRepository = walletRepository;
		_logger = logger;
	}


	/// <summary>
	/// Создать платеж
	/// </summary>
	public async Task<Result<PaymentDS, Error>> CreatePaymentAsync(Guid userId, Guid walletId, decimal amount, string description = "", CancellationToken ct = default)
	{
		try
		{
			_logger.LogInformation($"YooKassa: Создание платежа - WalletId: {walletId}, Amount: {amount}");


			// Создать локальный платеж
			var payment = PaymentDS.Create(walletId, userId, amount, description);

			// Создать платеж в YooKassa (версия 4.3.1)
			var newPayment = new NewPayment
			{
				Amount = new Amount
				{
					Value = amount,
					Currency = CurrencyCodes.RUB.ToString()
				},
				Confirmation = new Confirmation
				{
					Type = ConfirmationType.Redirect,
					ReturnUrl = _settings.SuccessReturnUrl
				},
				Description = description,
				Metadata = new Dictionary<string, string>
				{
					{ "wallet_id", walletId.ToString() },
					{ "user_id", userId.ToString() },
					{ "payment_id", payment.Id.ToString() }
				}
			};

			// Вызвать API YooKassa
			

			// Сохранить ID платежа
			payment.AggregatorPaymentId = yooKassaPayment.Id;

			// Добавить в БД
			var addResult = await _paymentRepository.AddAsync(payment, ct);

			if (addResult.IsFailure)
			{
				_logger.LogError("YooKassa: Ошибка при сохранении платежа в БД");
				return Error.Internal("error.save.payment", "Внутренняя ошибка сервера");
			}

			_logger.LogInformation(
				$"YooKassa: Платеж создан - PaymentId: {payment.Id}, " +
				$"YooKassaPaymentId: {yooKassaPayment.Id}, Status: {yooKassaPayment.Status}");

			return payment;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "YooKassa: Ошибка при создании платежа");
			return Error.Internal("error.create.payment", "Внутренняя ошибка сервера");
		}
	}


	/// <summary>
	/// Получить платеж по ID YooKassa
	/// </summary>
	public async Task<Result<PaymentDS, Error>> GetPaymentByYooKassaIdAsync(string yooKassaPaymentId, CancellationToken ct = default)
	{
		return await _paymentRepository.GetByAggregatorIdAsync(yooKassaPaymentId, ct);
	}


	/// <summary>
	/// Обновить статус платежа
	/// </summary>
	public async Task UpdatePaymentStatusAsync(Guid paymentId, PaymentStatus status, CancellationToken ct = default)
	{
		var paymentResult = await _paymentRepository.GetByIdAsync(paymentId, ct);
		if (paymentResult.IsSuccess)
		{
			var payment = paymentResult.Value;
			payment.Status = status;
			payment.UpdatedAt = DateTime.UtcNow;
			await _paymentRepository.UpdateAsync(payment);
		}
	}


	/// <summary>
	/// Завершить платеж
	/// </summary>
	public async Task CompletePaymentAsync(Guid paymentId, CancellationToken ct = default)
	{
		var paymentResult = await _paymentRepository.GetByIdAsync(paymentId, ct);
		if (paymentResult.IsFailure)
			return;

		var payment = paymentResult.Value;

		payment.MarkAsSucceeded();

		var wallet = await _walletRepository.GetByIdAsync(payment.WalletId, ct);
		if (wallet != null)
		{
			wallet.Balance += payment.Amount;
			await _walletRepository.UpdateAsync(wallet, ct);
		}

		_logger.LogInformation($"YooKassa: Платеж завершен - PaymentId: {paymentId}, Amount: {payment.Amount}");
	}


	/// <summary>
	/// Отменить платеж
	/// </summary>
	public async Task CancelPaymentAsync(Guid paymentId, string? reason = null, CancellationToken ct = default)
	{
		var paymentResult = await _paymentRepository.GetByIdAsync(paymentId, ct);
		if (paymentResult.IsFailure)
			return;

		var payment = paymentResult.Value;

		payment.MarkAsCanceled(reason);
		await _paymentRepository.UpdateAsync(payment);

		_logger.LogInformation($"YooKassa: Платеж отменен - PaymentId: {paymentId}");
	}


	

	/// <summary>
	/// Получить ссылку на оплату
	/// </summary>
	public async Task<string?> GetPaymentConfirmationUrlAsync(Guid paymentId, CancellationToken ct = default)
	{
		var paymentResult = await _paymentRepository.GetByIdAsync(paymentId, ct);
		if (paymentResult.IsFailure || string.IsNullOrEmpty(paymentResult.Value.AggregatorPaymentId))
			return null;

		try
		{
			var yooKassaPayment = _client.GetPayment(paymentResult.Value.AggregatorPaymentId);
			return yooKassaPayment?.Confirmation?.ConfirmationUrl;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "YooKassa: Ошибка при получении ссылки на оплату");
			return null;
		}
	}
}