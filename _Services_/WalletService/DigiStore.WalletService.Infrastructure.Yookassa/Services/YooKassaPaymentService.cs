using CSharpFunctionalExtensions;
using DigiStore.Enums;
using DigiStore.WalletService.Application.Configurations;
using DigiStore.WalletService.Application.Interfaces;
using DigiStore.WalletService.Domain;
using Microsoft.Extensions.Logging;
using Yandex.Checkout.V3;
using Error = DigiStore.SharedKernel.Error;
using PaymentStatus = DigiStore.Enums.PaymentStatus;

namespace DigiStore.WalletService.Infrastructure.Yookassa.Services;

/// <summary>
/// Сервис управления платежами YooKassa v4.3.1
/// </summary>
public class YooKassaPaymentService : IPaymentService
{
	private readonly Client _client;
	private readonly YooKassaSettings _settings;
    private readonly IPaymentRepository _paymentRepository;
    private readonly PaymentValidator _validator;
	private readonly ILogger<YooKassaPaymentService> _logger;

	public YooKassaPaymentService(
		Client client,
		YooKassaSettings settings,
		IPaymentRepository paymentRepository,
		ILogger<YooKassaPaymentService> logger)
	{
		_client = client;
		_settings = settings;
        _paymentRepository = paymentRepository;
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
            Payment yooKassaPayment = _client.CreatePayment(newPayment);

			if (yooKassaPayment == null)
				return Error.NotFound("error.create.payment", "Не удалось создать платеж в YooKassa");

			// Сохранить ID платежа
			payment.AggregatorPaymentId = yooKassaPayment.Id;

			// Добавить в БД
			var addResult = await _paymentRepository.AddAsync(payment, ct);

			if(addResult.IsFailure)
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
	/// Получить платеж по ID
	/// </summary>
	public async Task<Result<PaymentDS, Error>> GetPaymentAsync(Guid paymentId, CancellationToken ct = default)
	{
		return await _paymentRepository.GetByIdAsync(paymentId, ct);
	}


	/// <summary>
	/// Получить платеж по ID YooKassa
	/// </summary>
	public async Task<Result<PaymentDS, Error>> GetPaymentByYooKassaIdAsync(string yooKassaPaymentId, CancellationToken ct = default)
	{
		return await _paymentRepository. _dbContext.Set<PaymentDS>()
			.FirstOrDefaultAsync(p => p.YooKassaPaymentId == yooKassaPaymentId);
	}


	/// <summary>
	/// Обновить статус платежа
	/// </summary>
	public async Task UpdatePaymentStatusAsync(Guid paymentId, PaymentStatus status, CancellationToken ct = default)
	{
		var payment = await GetPaymentAsync(paymentId, ct);
		if (payment != null)
		{
			payment.Status = status;
			payment.UpdatedAt = DateTime.UtcNow;
			await _dbContext.SaveChangesAsync();
		}
	}


	/// <summary>
	/// Завершить платеж
	/// </summary>
	public async Task CompletePaymentAsync(Guid paymentId, CancellationToken ct = default)
	{
		var payment = await GetPaymentAsync(paymentId, ct);
		if (payment == null)
			return;

		payment.MarkAsSucceeded();

		var wallet = await _dbContext.Set<WalletDS>()
			.FirstOrDefaultAsync(w => w.Id == payment.WalletId);
		if (wallet != null)
		{
			wallet.Balance += payment.Amount;
		}

		await _dbContext.SaveChangesAsync();

		_logger.LogInformation(
			$"YooKassa: Платеж завершен - PaymentId: {paymentId}, Amount: {payment.Amount}");
	}


	/// <summary>
	/// Отменить платеж
	/// </summary>
	public async Task CancelPaymentAsync(Guid paymentId, string? reason = null, CancellationToken ct = default)
	{
		var payment = await GetPaymentAsync(paymentId, ct);
		if (payment == null)
			return;

		payment.MarkAsCanceled(reason);
		await _dbContext.SaveChangesAsync();

		_logger.LogInformation($"YooKassa: Платеж отменен - PaymentId: {paymentId}");
	}


	/// <summary>
	/// Получить платежи пользователя
	/// </summary>
	public async Task<Result<IReadOnlyList<PaymentDS>, Error>> GetUserPaymentsAsync(Guid userId, int skip = 0, int take = 10, CancellationToken ct = default)
	{
		return await _paymentRepository.GetUserPaymentsAsync(userId, skip, take, ct);
	}


	/// <summary>
	/// Получить ссылку на оплату
	/// </summary>
	public async Task<string?> GetPaymentConfirmationUrlAsync(Guid paymentId, CancellationToken ct = default)
	{
		var payment = await GetPaymentAsync(paymentId, ct);
		if (payment == null || string.IsNullOrEmpty(payment.AggregatorPaymentId))
			return null;

		try
		{
			var yooKassaPayment = _client.GetPayment(payment.AggregatorPaymentId);
			return yooKassaPayment?.Confirmation?.ConfirmationUrl;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "YooKassa: Ошибка при получении ссылки на оплату");
			return null;
		}
	}
}