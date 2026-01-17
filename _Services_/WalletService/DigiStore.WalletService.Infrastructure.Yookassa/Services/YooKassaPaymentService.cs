using CSharpFunctionalExtensions;
using DigiStore.Enums;
using DigiStore.WalletService.Application.Configurations;
using Microsoft.Extensions.Logging;
using Yandex.Checkout.V3;
using Error = DigiStore.SharedKernel.Error;

namespace DigiStore.WalletService.Infrastructure.Yookassa.Services;

/// <summary>
/// Сервис управления платежами YooKassa v4.3.1
/// </summary>
public class YooKassaPaymentService
{
	private readonly Client _client;
	private readonly YooKassaSettings _settings;
	private readonly ILogger<YooKassaPaymentService> _logger;

	public YooKassaPaymentService(
		Client client,
		YooKassaSettings settings,
		ILogger<YooKassaPaymentService> logger)
	{
		_client = client;
		_settings = settings;
        _logger = logger;
	}


	/// <summary>
	/// Создать платеж
	/// </summary>
	public async Task<Result<string, Error>> CreatePaymentAsync(Guid userId, Guid walletId, Guid paymentId, decimal amount, string description = "", CancellationToken ct = default)
	{
		try
		{
			_logger.LogInformation($"YooKassa: Создание платежа - WalletId: {walletId}, Amount: {amount}");

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
					{ "payment_id", paymentId.ToString() }
				}
			};

            // Вызвать API YooKassa
            Payment yooKassaPayment = _client.CreatePayment(newPayment);

			if (yooKassaPayment == null)
				return Error.NotFound("error.create.payment", "Не удалось создать платеж в YooKassa");

			_logger.LogInformation(
				$"YooKassa: Платеж создан - PaymentId: {paymentId}, " +
				$"YooKassaPaymentId: {yooKassaPayment.Id}, Status: {yooKassaPayment.Status}");

			return yooKassaPayment.Id;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "YooKassa: Ошибка при создании платежа");
			return Error.Internal("error.create.payment", "Внутренняя ошибка сервера");
		}
	}


	/// <summary>
	/// Получить ссылку на оплату
	/// </summary>
	public async Task<Result<string, Error>> GetPaymentConfirmationUrlAsync(string aggregatorPaymentId, CancellationToken ct = default)
	{
		try
		{
			var yooKassaPayment = _client.GetPayment(aggregatorPaymentId);
			var url = yooKassaPayment?.Confirmation?.ConfirmationUrl;

			return String.IsNullOrEmpty(url) ? Error.NotFound("error.payment.url.not.found", "Ссылка на оплату не найдена") : url;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "YooKassa: Ошибка при получении ссылки на оплату");
			return Error.Failure("", "Ошибка при получении ссылки на оплату");
		}
	}
}