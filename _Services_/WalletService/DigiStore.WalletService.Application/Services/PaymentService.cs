using CSharpFunctionalExtensions;
using DigiStore.Enums;
using DigiStore.SharedKernel;
using DigiStore.TgBot.Contracts.HttpClients;
using DigiStore.TgBot.Contracts.Requests;
using DigiStore.WalletService.Application.DTOs;
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
	private readonly IWalletUnitOfWork _unitOfWork;
    private readonly IYookassaProvider _yookassaProvider;
    private readonly ITgBotHttpClient _tgBotHttpClient;
    private readonly ILogger<PaymentService> _logger;

	public PaymentService(
		IPaymentRepository paymentRepository,
		IWalletRepository walletRepository,
		IWalletUnitOfWork unitOfWork,
		IYookassaProvider yookassaProvider,
		ITgBotHttpClient tgBotHttpClient,
		ILogger<PaymentService> logger)
	{
		_paymentRepository = paymentRepository;
		_walletRepository = walletRepository;
		_unitOfWork = unitOfWork;
        _yookassaProvider = yookassaProvider;
        _tgBotHttpClient = tgBotHttpClient;
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
			payment.Status = PaymentStatus.Pending;
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
	public async Task<UnitResult<Error>> CompletePaymentAsync(PaymentSuccessDTO paymentSuccessDTO, CancellationToken token)
	{
		var txResult = await _unitOfWork.BeginTransactionAsync(token);
		if (txResult.IsFailure)
			return txResult.Error;

		await using var tx = txResult.Value;

		try
		{
			// STEP 1: Получить платеж из БД (с UPDLOCK)
			var paymentResult = await _paymentRepository.GetByAggregatorIdForUpdateAsync(paymentSuccessDTO.AggregatorPaymentId, token);
			if (paymentResult.IsFailure)
			{
				await tx.RollbackAsync(token);
				return paymentResult.Error;
			}

			var payment = paymentResult.Value;

			// STEP 2: Проверить идемпотентность (не обработали ли уже?)
			if (payment.Status == PaymentStatus.Succeeded)
			{
				await tx.CommitAsync(token);
				_logger.LogInformation("YooKassa: Идемпотентность - платеж уже завершен - PaymentId: {PaymentId}", payment.Id);
				return Result.Success<Error>();
			}

			if (payment.Status == PaymentStatus.Canceled)
			{
				await tx.RollbackAsync(token);
				return Error.Conflict("payment.canceled", "Платеж отменен и не может быть завершен");
			}

			// STEP 3: Проверить консистентность сумм
			if (payment.Amount != paymentSuccessDTO.Amount)
			{
				await tx.RollbackAsync(token);
				_logger.LogError("Amount mismatch: DB={DbAmount}, YooKassa={YooKassaAmount}", payment.Amount, paymentSuccessDTO.Amount);
				return Error.Failure("payment.amount.invalid", "Некорректная сумма платежа");
			}

			if (paymentSuccessDTO.MetaData?.WalletId != payment.WalletId)
			{
				await tx.RollbackAsync(token);
				_logger.LogError("Metadata mismatch: Meta={MetaWalletId}, DB={WalletId}", paymentSuccessDTO.MetaData?.WalletId, payment.WalletId);
				return Error.Failure("payment.walletid.invalid", "Некорректный walletid");
			}

			if (paymentSuccessDTO.MetaData?.UserId != payment.UserId)
			{
				await tx.RollbackAsync(token);
				_logger.LogError("Metadata mismatch: Meta={MetaUserId}, DB={UserId}", paymentSuccessDTO.MetaData?.UserId, payment.UserId);
				return Error.Failure("payment.userid.invalid", "Некорректный userid");
			}

			if (paymentSuccessDTO.MetaData?.PaymentId != payment.Id)
			{
				await tx.RollbackAsync(token);
				_logger.LogError("Metadata mismatch: Meta={MetaPaymentId}, DB={PaymentId}", paymentSuccessDTO.MetaData?.PaymentId, payment.Id);
				return Error.Failure("payment.paymentid.invalid", "Некорректный paymentid");
			}

			// STEP 4: Получить кошелек (с UPDLOCK)
			var walletResult = await _walletRepository.GetByIdForUpdateAsync(payment.WalletId, token);
			if (walletResult.IsFailure)
			{
				await tx.RollbackAsync(token);
				return walletResult.Error;
			}

			var wallet = walletResult.Value;

			if (wallet.IsFrozen)
			{
				_logger.LogWarning("Wallet is frozen: WalletId={WalletId}", wallet.Id);
				return Error.Failure("wallet.frozen", "Кошелек заморожен");
			}


			// STEP 5: Обновить статус платежа
			payment.MarkAsSucceeded(paymentSuccessDTO.PaymentMethod);

			// STEP 6: Обновить баланс кошелька
			wallet.Balance += payment.Amount;
			wallet.TotalDeposited += payment.Amount;
			wallet.UpdatedAt = DateTime.UtcNow;

			// STEP 7: Создать транзакцию (история)
			var walletTransaction = new TransactionDS
			{
				Id = Guid.NewGuid(),
				WalletId = wallet.Id,
				UserId = wallet.UserId,
				Amount = payment.Amount,
				Type = TransactionTypes.Deposit,
				Status = TransactionStatuses.Completed,
				Description = payment.Description,
				ReferenceId = payment.Id.ToString(),
				ReferenceType = nameof(PaymentDS),
				BalanceAfter = wallet.Balance,
				PaymentMethod = payment.PaymentMethod,
				CreatedAt = DateTime.UtcNow
			};

			// STEP 8: Ссылаем платеж на транзакцию
			payment.TransactionId = walletTransaction.Id;
			payment.UpdatedAt = DateTime.UtcNow;

			// STEP 9: Сохранить все изменения АТОМАРНО
			var addTxResult = await _walletRepository.AddTransactionAsync(walletTransaction, token);
			if (addTxResult.IsFailure)
			{
				await tx.RollbackAsync(token);
				return addTxResult.Error;
			}

			var saveWalletResult = await _walletRepository.SaveChangesAsync(token);
			if (saveWalletResult.IsFailure)
			{
				await tx.RollbackAsync(token);
				return saveWalletResult.Error;
			}

			var savePaymentResult = await _paymentRepository.SaveChangesAsync(token);
			if (savePaymentResult.IsFailure)
			{
				await tx.RollbackAsync(token);
				return savePaymentResult.Error;
			}

			// STEP 10: Коммитим транзакцию БД
			await tx.CommitAsync(token);

			_logger.LogInformation("YooKassa: Платеж завершен - PaymentId: {PaymentId}, Amount: {Amount}, TransactionId: {TransactionId}", payment.Id, payment.Amount, walletTransaction.Id);


			// Отправляем webhook TG боту для изменения сообщения
			return await _tgBotHttpClient.UpdatePaymentAsync(payment.UserId, new UpdatePaymentRequest(), token);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "YooKassa: Ошибка при завершении платежа - AggregatorPaymentId: {AggregatorPaymentId}", paymentSuccessDTO.AggregatorPaymentId);
			try
			{
				await tx.RollbackAsync(token);
			}
			catch
			{
				// ignore rollback exceptions
			}

			return Error.Internal("payment.complete.failed", "Внутренняя ошибка сервера");
		}
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