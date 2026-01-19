using CSharpFunctionalExtensions;
using DigiStore.Enums;
using DigiStore.WalletService.Application.Configurations;
using DigiStore.WalletService.Application.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Yandex.Checkout.V3;
using Error = DigiStore.SharedKernel.Error;

namespace DigiStore.WalletService.Infrastructure.Yookassa.Services;

/// <summary>
/// Сервис управления платежами YooKassa v4.3.1
/// </summary>
public class YookassaProvider : IYookassaProvider
{
	private readonly AsyncClient _clientYooKassa;
	private readonly YooKassaSettings _settings;
	private readonly ILogger<YookassaProvider> _logger;

	public YookassaProvider(
		Client client,
		IOptions<YooKassaSettings> settings,
		ILogger<YookassaProvider> logger)
	{
		_clientYooKassa = client.MakeAsync();
		_settings = settings.Value;
		_logger = logger;
	}


	/// <summary>
	/// Создать платеж
	/// </summary>
	public async Task<Result<string, Error>> CreatePaymentAsync(
		Guid userId, 
		Guid walletId, 
		Guid paymentId, 
		decimal amount, 
		string description = "",
		string username = "",
		CancellationToken token = default)
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
					ReturnUrl = $"{_settings.SuccessReturnUrl}/{username}"
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
			Payment yooKassaPayment = await _clientYooKassa.CreatePaymentAsync(newPayment, cancellationToken: token);

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
	public async Task<Result<string, Error>> GetPaymentConfirmationUrlAsync(string aggregatorPaymentId, CancellationToken token)
	{
		try
		{
			var yooKassaPayment = await _clientYooKassa.GetPaymentAsync(aggregatorPaymentId, cancellationToken: token);
			var url = yooKassaPayment?.Confirmation?.ConfirmationUrl;

			return String.IsNullOrEmpty(url) ? Error.NotFound("error.payment.url.not.found", "Ссылка на оплату не найдена") : url;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "YooKassa: Ошибка при получении ссылки на оплату");
			return Error.Failure("", "Ошибка при получении ссылки на оплату");
		}
	}





	/// <summary>
	/// Создать выплату на карту
	/// </summary>
	public async Task<Result<string, Error>> CreateWithdrawalAsync(
		Guid walletId,
		Guid withdrawalId,
		decimal amount,
		decimal actualAmount,
		CancellationToken token)
	{
		try
		{
			_logger.LogInformation($"YooKassa: Создание выплаты - WalletId: {walletId}, Amount: {amount}");

			// Создать выплату в YooKassa
			// В версии 4.3.1 используется другой API для выплат
			var newPayout = new NewPayout
			{
				Amount = new Amount
				{
					Value = actualAmount,
					Currency = CurrencyCodes.RUB.ToString()
				},
				// Для карты используется идентификатор платежного средства
				// или прямой номер карты (в зависимости от конфигурации)
			};

			// ВАЖНО: В версии 4.3.1 выплаты требуют другую конфигурацию
			// Возможно, нужно использовать Direct API или Custom API
			// 
			// Вариант 1: Если у вас есть сохраненное платежное средство
			// newPayout.PaymentInstrumentId = savedPaymentMethodId;
			//
			// Вариант 2: Если выплаты не поддерживаются напрямую
			// нужно использовать другой endpoint или сервис

			// Попытаемся создать выплату
			Payout? yooKassaPayout = await _clientYooKassa.CreatePayoutAsync(newPayout, cancellationToken: token);

			if (yooKassaPayout == null)
				return Error.Failure("withdrawal.fail", "Не удалось создать выплату в YooKassa");

			_logger.LogInformation(
				$"YooKassa: Выплата создана - WithdrawalId: {withdrawalId}, " +
				$"YooKassaWithdrawalId: {yooKassaPayout.Id}, Status: {yooKassaPayout.Status}");

			return yooKassaPayout.Id;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "YooKassa: Ошибка при создании выплаты в версии 4.3.1");

			// Проверяем, что это за ошибка
			if (ex.Message.Contains("Recipient") || ex.Message.Contains("recipient"))
			{
				return Error.Failure("withdrawal.fail.recipient", "Внутренняя ошибка сервера при создании выплаты");
			}

			return Error.Failure("withdrawal.fail", "Внутренняя ошибка сервера при создании выплаты");
		}
	}


	public async Task<Result<string, Error>> CapturePaymentAsync(string paymentId, CancellationToken token)
	{
		// 4. Подтвердите готовность принять платеж
		var payResult = await _clientYooKassa.CapturePaymentAsync(paymentId, cancellationToken: token);

		return payResult.Id;
	}
}