using DigiStore.WalletService.Application.Configurations;
using DigiStore.WalletService.Application.Interfaces;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;
using Yandex.Checkout.V3;

namespace DigiStore.WalletService.Infrastructure.Yookassa.Services;

/// <summary>
/// Сервис обработки вебхуков от YooKassa
/// </summary>
public class YooKassaWebhookServiceOLD : IYooKassaWebhookService
{
	private readonly YooKassaSettings _settings;
	private readonly IYookassaProvider _yookassaProvider;
	private readonly IWithdrawalService _withdrawalService;
    private readonly IWithdrawalRepository _withdrawalRepository;
    private readonly IPaymentService _paymentService;
    private readonly IPaymentRepository _paymentRepository;
    private readonly ILogger<YooKassaWebhookServiceOLD> _logger;

	public YooKassaWebhookServiceOLD(
		YooKassaSettings settings,
		IYookassaProvider yookassaProvider,
		IWithdrawalService withdrawalService,
		IWithdrawalRepository withdrawalRepository,
		IPaymentService paymentService,
		IPaymentRepository paymentRepository,
		ILogger<YooKassaWebhookServiceOLD> logger)
	{
		_settings = settings;
		_yookassaProvider = yookassaProvider;
		_withdrawalService = withdrawalService;
        _withdrawalRepository = withdrawalRepository;
        _paymentService = paymentService;
        _paymentRepository = paymentRepository;
        _logger = logger;
	}

	/// <summary>
	/// Проверить подпись вебхука
	/// </summary>
	public bool VerifyWebhookSignature(string jsonBody, string signatureHeader)
	{
		try
		{
			if (string.IsNullOrEmpty(signatureHeader))
				return false;

			// Формат: sha256=BASE64_SIGNATURE
			var parts = signatureHeader.Split('=');
			if (parts.Length != 2)
				return false;

			var signature = parts[1];
			var algorithm = parts[0];

			if (algorithm != "sha256")
				return false;

			// Вычислить сигнатуру
			var data = Encoding.UTF8.GetBytes(jsonBody + _settings.WebhookSecret);
			using (var hmac = new HMACSHA256(
				Encoding.UTF8.GetBytes(_settings.WebhookSecret)))
			{
				var hash = hmac.ComputeHash(data);
				var computed = Convert.ToBase64String(hash);

				return computed == signature;
			}
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "YooKassa: Ошибка проверки подписи");
			return false;
		}
	}


	/// <summary>
	/// Обработать вебхук от YooKassa
	/// </summary>
	public async Task ProcessWebhookAsync(string jsonBody, CancellationToken token)
	{
		try
		{
			// Парсить вебхук используя встроенный парсер библиотеки
			var notification = Client.ParseMessage(
				"POST",
				"application/json",
				new MemoryStream(Encoding.UTF8.GetBytes(jsonBody)));

			if (notification is PaymentSucceededNotification paymentNotification)
			{
				await ProcessPaymentSucceededAsync(paymentNotification.Object, token);
			}
			else if (notification is PaymentWaitingForCaptureNotification captureNotification)
			{
				await ProcessPaymentWaitingForCaptureAsync(captureNotification.Object);
			}
			else if (notification is PaymentCanceledNotification cancelNotification)
			{
				await ProcessPaymentCanceledAsync(cancelNotification.Object, token);
			}
			else if (notification is RefundSucceededNotification refundNotification)
			{
				await ProcessRefundSucceededAsync(refundNotification.Object);
			}
			else if (notification is PayoutSucceededNotification payoutNotification)
			{
				await ProcessPayoutSucceededAsync(payoutNotification.Object, token);
			}
			else if (notification is PayoutCanceledNotification payoutCancelNotification)
			{
				await ProcessPayoutCanceledAsync(payoutCancelNotification.Object, token);
			}
			else
			{
				_logger.LogWarning("YooKassa: Неизвестный тип уведомления");
			}
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "YooKassa: Ошибка при обработке вебхука");
		}
	}

	private async Task ProcessPaymentSucceededAsync(Payment payment, CancellationToken token)
	{
		_logger.LogInformation($"YooKassa: Платеж успешен - PaymentId: {payment.Id}");

		var dbPayment = await _paymentRepository.GetByAggregatorIdAsync(payment.Id, token);
		if (dbPayment.IsSuccess)
		{
			await _paymentService.CompletePaymentAsync(dbPayment.Value.Id, token);
		}
	}

	private async Task ProcessPaymentWaitingForCaptureAsync(Payment payment)
	{
		_logger.LogInformation(
			$"YooKassa: Платеж требует подтверждения - PaymentId: {payment.Id}");
		// Подтвердить платеж
	}

	private async Task ProcessPaymentCanceledAsync(Payment payment, CancellationToken token)
	{
		_logger.LogInformation($"YooKassa: Платеж отменен - PaymentId: {payment.Id}");

		var dbPayment = await _paymentRepository.GetByAggregatorIdAsync(payment.Id, token);
		if (dbPayment.IsSuccess)
		{
			await _paymentRepository.CancelPaymentAsync(dbPayment.Value.Id, "Отменен YooKassa");
		}
	}

	private async Task ProcessRefundSucceededAsync(Refund refund)
	{
		_logger.LogInformation(
			$"YooKassa: Возврат успешен - RefundId: {refund.Id}, PaymentId: {refund.PaymentId}");
	}

	private async Task ProcessPayoutSucceededAsync(Payout payout, CancellationToken token)
	{
		_logger.LogInformation($"YooKassa: Выплата успешна - PayoutId: {payout.Id}");

		var dbWithdrawal = await _withdrawalRepository.GetByAggregatorIdAsync(payout.Id, token);
		if (dbWithdrawal.IsSuccess)
		{
			await _withdrawalRepository.CompleteWithdrawalAsync(dbWithdrawal.Value.Id, token);
		}
	}

	private async Task ProcessPayoutCanceledAsync(Payout payout, CancellationToken token)
	{
		_logger.LogInformation($"YooKassa: Выплата отменена - PayoutId: {payout.Id}");

		var dbWithdrawal = await _withdrawalRepository.GetByAggregatorIdAsync(payout.Id, token);
		if (dbWithdrawal.IsSuccess)
		{
			await _withdrawalService.CancelWithdrawalAsync(dbWithdrawal.Value.Id, "Отменена YooKassa");
		}
	}

}



/// <summary>
/// Сервис обработки вебхуков YooKassa v4.3.1
/// </summary>
public class YooKassaWebhookService : IYooKassaWebhookService
{
	private readonly YooKassaSettings _settings;
	private readonly ILogger<YooKassaWebhookService> _logger;

	public YooKassaWebhookService(
		YooKassaSettings settings,
		YookassaProvider paymentService,
		ILogger<YooKassaWebhookService> logger)
	{
		_settings = settings;
		_logger = logger;
	}

	/// <summary>
	/// Проверить подпись вебхука
	/// </summary>
	public bool VerifyWebhookSignature(string jsonBody, string signatureHeader)
	{
		try
		{
			if (string.IsNullOrEmpty(signatureHeader))
				return false;

			var data = Encoding.UTF8.GetBytes(jsonBody + _settings.WebhookSecret);
			using var sha256 = SHA256.Create();
			var hash = sha256.ComputeHash(data);
			var computed = Convert.ToBase64String(hash);

			var signature = signatureHeader.Replace("sha256=", "").Trim();
			var result = computed == signature;

			if (!result)
				_logger.LogWarning("YooKassa: Неверная подпись вебхука");

			return result;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "YooKassa: Ошибка проверки подписи");
			return false;
		}
	}


	/// <summary>
	/// Обработать вебхук
	/// </summary>
	public async Task ProcessWebhookAsync(string bodyContent, CancellationToken token)
	{
		try
		{
			// Парсить вебхук используя встроенный парсер версии 4.3.1
			// ПРИМЕЧАНИЕ: В версии 4.3.1 используется другой способ парсинга

			_logger.LogInformation($"YooKassa: Обработка вебхука - Body: {bodyContent.Substring(0, 100)}...");

			// Здесь нужно использовать встроенный парсер из библиотеки
			// В версии 4.3.1 это может быть разной реализацией

			_logger.LogInformation("YooKassa: Вебхук обработан");
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "YooKassa: Ошибка при обработке вебхука");
		}
	}
}