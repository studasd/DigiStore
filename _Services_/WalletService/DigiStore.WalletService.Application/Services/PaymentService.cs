using CSharpFunctionalExtensions;
using DigiStore.SharedKernel;
using DigiStore.WalletService.Application.Interfaces;
using DigiStore.WalletService.Domain;
using Microsoft.Extensions.Logging;

namespace DigiStore.WalletService.Application.Services;


/// <summary>
/// Сервис управления платежами
/// </summary>
public class PaymentService : IPaymentService
{
	private readonly IPaymentRepository _paymentRepository;
	private readonly IWalletRepository _walletRepository;
    private readonly IYookassaProvider _yookassaProvider;
    private readonly ILogger<PaymentService> _logger;

	public PaymentService(
		IPaymentRepository paymentRepository,
		IWalletRepository walletRepository,
		IYookassaProvider yookassaProvider,
		ILogger<PaymentService> logger)
	{
		_paymentRepository = paymentRepository;
		_walletRepository = walletRepository;
        _yookassaProvider = yookassaProvider;
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
			var yooKassaPaymentResult = await _yookassaProvider.CreatePaymentAsync(userId, walletId, payment.Id, amount, description, ct);

            if (yooKassaPaymentResult.IsFailure)
            {
				return yooKassaPaymentResult.Error;
			}

            payment.AggregatorPaymentId = yooKassaPaymentResult.Value;


			// Добавить в БД
			var addResult = await _paymentRepository.AddAsync(payment, ct);

			if (addResult.IsFailure)
			{
				_logger.LogError("YooKassa: Ошибка при сохранении платежа в БД");
				return Error.Internal("error.save.payment", "Внутренняя ошибка сервера");
			}

			return payment;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "YooKassa: Ошибка при создании платежа");
			return Error.Internal("error.create.payment", "Внутренняя ошибка сервера");
		}
	}


	/// <summary>
	/// Завершить платеж
	/// </summary>
	public async Task<UnitResult<Error>> CompletePaymentAsync(Guid paymentId, CancellationToken ct = default)
	{
		var paymentResult = await _paymentRepository.GetByIdAsync(paymentId, ct);
		if (paymentResult.IsFailure)
			return paymentResult.Error;

		var payment = paymentResult.Value;

		payment.MarkAsSucceeded();

		var walletResult = await _walletRepository.GetByIdAsync(payment.WalletId, ct);
		if (walletResult.IsFailure)
			return walletResult.Error;

		var wallet = walletResult.Value;

		wallet.Balance += payment.Amount;
		var updateResult = await _walletRepository.UpdateAsync(wallet, ct);
        if(updateResult.IsFailure)
			return updateResult.Error;

        _logger.LogInformation($"YooKassa: Платеж завершен - PaymentId: {paymentId}, Amount: {payment.Amount}");
		return Result.Success<Error>();
	}



	/// <summary>
	/// Получить ссылку на оплату
	/// </summary>
	public async Task<Result<string, Error>> GetPaymentConfirmationUrlAsync(Guid paymentId, CancellationToken ct = default)
	{
		var paymentResult = await _paymentRepository.GetByIdAsync(paymentId, ct);
		if (paymentResult.IsFailure || string.IsNullOrEmpty(paymentResult.Value.AggregatorPaymentId))
			return paymentResult.Error;

		var confirmUrlResult = await _yookassaProvider.GetPaymentConfirmationUrlAsync(paymentResult.Value.AggregatorPaymentId, ct);

		if (confirmUrlResult.IsFailure)
			return confirmUrlResult.Error;

		return confirmUrlResult.Value;
	}
}