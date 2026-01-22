using CSharpFunctionalExtensions;
using DigiStore.TgBot.Contracts.HttpClients;
using DigiStore.TgBot.Contracts.Requests;
using DigiStore.WalletService.Application.Configurations;
using DigiStore.WalletService.Application.DTOs;
using DigiStore.WalletService.Application.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text;
using Yandex.Checkout.V3;
using Error = DigiStore.SharedKernel.Error;

namespace DigiStore.WalletService.Infrastructure.Yookassa.Services;

/// <summary>
/// Сервис обработки вебхуков от YooKassa
/// </summary>
public class YooKassaWebhookService : IYooKassaWebhookService
{
	private readonly YooKassaSettings _settings;
	private readonly IYookassaProvider _yookassaProvider;
	private readonly IWithdrawalService _withdrawalService;
    private readonly IWithdrawalRepository _withdrawalRepository;
    private readonly IPaymentService _paymentService;
    private readonly IPaymentRepository _paymentRepository;
	private readonly ITgBotHttpClient _tgBotHttpClient;
    private readonly ILogger<YooKassaWebhookService> _logger;

	public YooKassaWebhookService(
		IOptions<YooKassaSettings> settings,
		IYookassaProvider yookassaProvider,
		IWithdrawalService withdrawalService,
		IWithdrawalRepository withdrawalRepository,
		IPaymentService paymentService,
		IPaymentRepository paymentRepository,
		ITgBotHttpClient tgBotHttpClient,
		ILogger<YooKassaWebhookService> logger)
	{
		_settings = settings.Value;
		_yookassaProvider = yookassaProvider;
		_withdrawalService = withdrawalService;
        _withdrawalRepository = withdrawalRepository;
        _paymentService = paymentService;
        _paymentRepository = paymentRepository;
		_tgBotHttpClient = tgBotHttpClient;
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
	public async Task<UnitResult<Error>> ProcessWebhookAsync(string jsonBody, CancellationToken token)
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
				return await ProcessPaymentSucceededAsync(paymentNotification.Object, token);
			}
			else if (notification is PaymentWaitingForCaptureNotification captureNotification)
			{
				return await ProcessPaymentWaitingForCaptureAsync(captureNotification.Object);
			}
			else if (notification is PaymentCanceledNotification cancelNotification)
			{
				return await ProcessPaymentCanceledAsync(cancelNotification.Object, token);
			}
			else if (notification is RefundSucceededNotification refundNotification)
			{
				return await ProcessRefundSucceededAsync(refundNotification.Object);
			}
			else if (notification is PayoutSucceededNotification payoutNotification)
			{
				return await ProcessPayoutSucceededAsync(payoutNotification.Object, token);
			}
			else if (notification is PayoutCanceledNotification payoutCancelNotification)
			{
				return await ProcessPayoutCanceledAsync(payoutCancelNotification.Object, token);
			}
			else
			{
				_logger.LogWarning("YooKassa: Неизвестный тип уведомления");
				return Error.NotFound("error.unknown.notification", "Неизвестный тип уведомления");
			}
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "YooKassa: Ошибка при обработке вебхука");
		}

		return Error.Failure("error.process.webhook", "Ошибка при обработке вебхука");
	}


	private async Task<UnitResult<Error>> ProcessPaymentSucceededAsync(Payment payment, CancellationToken token)
	{
		if (payment.Status == PaymentStatus.Succeeded) 
		{
			_logger.LogInformation($"YooKassa: Платеж успешен - PaymentId: {payment.Id}");

			if (!payment.Metadata.ContainsKey("wallet_id") || !Guid.TryParse(payment.Metadata["wallet_id"], out Guid walletId))
				walletId = default;

			if (!payment.Metadata.ContainsKey("user_id") || !Guid.TryParse(payment.Metadata["user_id"], out Guid userId))
				userId = default;

			if (!payment.Metadata.ContainsKey("payment_id") || !Guid.TryParse(payment.Metadata["payment_id"], out Guid paymentId))
				paymentId = default;

			var metaData = new PaymentMetaDTO(walletId, userId, paymentId);

			var dto = new PaymentSuccessDTO(
				payment.Id,
				payment.Amount.Value,
				payment.PaymentMethod.Type,
				metaData
				);

			var completeResult = await _paymentService.CompletePaymentAsync(dto, token);
			if(completeResult.IsFailure)
				return completeResult.Error;

			// Отправляем webhook TG боту для изменения сообщения
			return await _tgBotHttpClient.UpdatePaymentAsync(userId, new UpdatePaymentRequest(paymentId), CancellationToken.None);
		}

		return Error.Failure("error.payment.not.succeeded", "Платеж не в статусе 'succeeded'");
	}


	private async Task<UnitResult<Error>> ProcessPaymentWaitingForCaptureAsync(Payment payment)
	{
		_logger.LogInformation($"YooKassa: Платеж требует подтверждения - PaymentId: {payment.Id}");
		
		// Подтвердить платеж
		return await _yookassaProvider.CapturePaymentAsync(payment.Id, token: CancellationToken.None);
	}


	private async Task<UnitResult<Error>> ProcessPaymentCanceledAsync(Payment payment, CancellationToken token)
	{
		_logger.LogInformation($"YooKassa: Платеж отменен - PaymentId: {payment.Id}");

		var dbPayment = await _paymentRepository.GetByAggregatorIdAsync(payment.Id, token);
		if (dbPayment.IsSuccess)
		{
			var cancelReason = $"YooKassa [{payment.CancellationDetails.Party} : {payment.CancellationDetails.Reason}]";
			var cancelResult = await _paymentRepository.CancelPaymentAsync(dbPayment.Value.Id, $"Отменен {cancelReason}", payment.PaymentMethod?.Type, token);
			if (cancelResult.IsFailure)
				return cancelResult;

			// Notify TgBot to update the original payment message
			return await _tgBotHttpClient.CancelPaymentAsync(dbPayment.Value.UserId, new CancelPaymentRequest(dbPayment.Value.Id, cancelReason), token);
		}

		return Error.Failure("error.payment.not.found", "Платеж не найден в БД");
	}


	private async Task<UnitResult<Error>> ProcessRefundSucceededAsync(Refund refund)
	{
		_logger.LogInformation($"YooKassa: Возврат успешен - RefundId: {refund.Id}, PaymentId: {refund.PaymentId}");
		return Result.Success<Error>();
	}


	private async Task<UnitResult<Error>> ProcessPayoutSucceededAsync(Payout payout, CancellationToken token)
	{
		_logger.LogInformation($"YooKassa: Выплата успешна - PayoutId: {payout.Id}");

		var dbWithdrawal = await _withdrawalRepository.GetByAggregatorIdAsync(payout.Id, token);
		if (dbWithdrawal.IsSuccess)
		{
			return await _withdrawalRepository.CompleteWithdrawalAsync(dbWithdrawal.Value.Id, token);
		}

		return Error.Failure("error.withdrawal.not.found", "Выплата не найдена в БД");
	}


	private async Task<UnitResult<Error>> ProcessPayoutCanceledAsync(Payout payout, CancellationToken token)
	{
		_logger.LogInformation($"YooKassa: Выплата отменена - PayoutId: {payout.Id}");

		var dbWithdrawal = await _withdrawalRepository.GetByAggregatorIdAsync(payout.Id, token);
		if (dbWithdrawal.IsSuccess)
		{
			return await _withdrawalService.CancelWithdrawalAsync(dbWithdrawal.Value.Id, "Отменена YooKassa");
		}

		return Error.Failure("error.withdrawal.not.found", "Выплата не найдена в БД");
	}
}