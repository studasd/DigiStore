using CSharpFunctionalExtensions;
using DigiStore.Enums;
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
	public async Task<Result<PaymentDS, Error>> CreatePaymentAsync(Guid userId, Guid walletId, decimal amount, PaymentAggregators aggregator, string description, string returnUrl, CancellationToken token = default)
	{
		_logger.LogInformation($"Создание платежа - WalletId: {walletId}, Amount: {amount}");


		// Создать локальный платеж
		var payment = PaymentDS.Create(walletId, userId, amount, aggregator, description, returnUrl);

		if (aggregator == PaymentAggregators.YooKassa)
		{
			// Создать платеж в YooKassa (версия 4.3.1)
			var yooKassaPaymentResult = await _yookassaProvider.CreatePaymentAsync(
				userId, 
				walletId, 
				payment.Id, 
				amount, 
				description,
				returnUrl,
				token);

			if (yooKassaPaymentResult.IsFailure)
			{
				return yooKassaPaymentResult.Error;
			}

			payment.AggregatorPaymentId = yooKassaPaymentResult.Value;
		}
		else
		{
			return Error.NotFound("error.aggregator.not.found", "Платежный агрегатор не найден");
		}


		// Добавить в БД
		var addResult = await _paymentRepository.AddAsync(payment, token);

		if (addResult.IsFailure)
		{
			_logger.LogError("YooKassa: Ошибка при сохранении платежа в БД");
			return Error.Internal("error.save.payment", "Внутренняя ошибка сервера");
		}

		return payment;
	}


	/// <summary>
	/// Завершить платеж
	/// </summary>
	public async Task<UnitResult<Error>> CompletePaymentAsync(Guid paymentId, CancellationToken token)
	{
		var paymentResult = await _paymentRepository.GetByIdAsync(paymentId, token);
		if (paymentResult.IsFailure)
			return paymentResult.Error;

		var payment = paymentResult.Value;

		payment.MarkAsSucceeded();

		var walletResult = await _walletRepository.GetByIdAsync(payment.WalletId, token);
		if (walletResult.IsFailure)
			return walletResult.Error;

		var wallet = walletResult.Value;

		wallet.Balance += payment.Amount;
		var updateResult = await _walletRepository.UpdateAsync(wallet, token);
        if(updateResult.IsFailure)
			return updateResult.Error;

        _logger.LogInformation($"YooKassa: Платеж завершен - PaymentId: {paymentId}, Amount: {payment.Amount}");
		return Result.Success<Error>();
	}



	/// <summary>
	/// Получить ссылку на оплату
	/// </summary>
	public async Task<Result<string, Error>> GetPaymentConfirmationUrlAsync(Guid paymentId, CancellationToken token)
	{
		var paymentResult = await _paymentRepository.GetByIdAsync(paymentId, token);
		if (paymentResult.IsFailure || string.IsNullOrEmpty(paymentResult.Value.AggregatorPaymentId))
			return paymentResult.Error;

		var confirmUrlResult = await _yookassaProvider.GetPaymentConfirmationUrlAsync(paymentResult.Value.AggregatorPaymentId, token);

		if (confirmUrlResult.IsFailure)
			return confirmUrlResult.Error;

		return confirmUrlResult.Value;
	}
}