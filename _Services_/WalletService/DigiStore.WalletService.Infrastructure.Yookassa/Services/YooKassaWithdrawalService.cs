using CSharpFunctionalExtensions;
using DigiStore.Enums;
using DigiStore.SharedKernel;
using DigiStore.WalletService.Application.Configurations;
using DigiStore.WalletService.Application.Interfaces;
using DigiStore.WalletService.Domain;
using DigiStore.WalletService.Domain.Enums;
using DigiStore.WalletService.Infrastructure.Yookassa.Validators;
using Microsoft.Extensions.Logging;
using Yandex.Checkout.V3;
using Error = DigiStore.SharedKernel.Error;

namespace DigiStore.WalletService.Infrastructure.Yookassa.Services;

/// <summary>
/// Сервис управления выплатами YooKassa
/// Обновлено для версии 4.3.1
/// </summary>
public class YooKassaWithdrawalService
{
	private readonly Client _yooKassaClient;
	private readonly YooKassaSettings _settings;
	private readonly WithdrawalValidator _validator;
    private readonly IWalletRepository _walletRepository;
    private readonly IWithdrawalRepository _withdrawalRepository;
    private readonly ILogger<YooKassaWithdrawalService> _logger;

	public YooKassaWithdrawalService(
		Client yooKassaClient,
		YooKassaSettings settings,
		WithdrawalValidator validator,
		IWalletRepository walletRepository,
		IWithdrawalRepository withdrawalRepository,
		ILogger<YooKassaWithdrawalService> logger)
	{
		_yooKassaClient = yooKassaClient;
		_settings = settings;
		_validator = validator;
        _walletRepository = walletRepository;
        _withdrawalRepository = withdrawalRepository;
        _logger = logger;
	}

	/// <summary>
	/// Создать выплату на карту
	/// </summary>
	public async Task<(bool Success, WithdrawalDS? Withdrawal, string? Error)> CreateWithdrawalAsync(
		Guid walletId,
		Guid userId,
		decimal amount,
		string cardNumber,
		CancellationToken ct)
	{
		try
		{
			_logger.LogInformation($"YooKassa: Создание выплаты - WalletId: {walletId}, Amount: {amount}");

			// Проверить кошелек
			var wallet = await _walletRepository.GetByIdAsync(walletId, ct);
			if (wallet == null)
				return (false, null, "Кошелек не найден");

			// Валидировать выплату
			var validation = _validator.ValidateWithdrawal(
				walletId, userId, wallet.Balance, amount, cardNumber);
			if (!validation.IsValid)
			{
				_logger.LogWarning($"YooKassa: Ошибка валидации - {validation.ErrorMessage}");
				return (false, null, validation.ErrorMessage);
			}

			// Создать локальную выплату
			var withdrawal = WithdrawalDS.Create(walletId, userId, amount);
			withdrawal.CardMask = MaskCardNumber(cardNumber);

			// Вычесть сумму со счета
			wallet.Balance -= amount;

			try
			{
				// Создать выплату в YooKassa
				// В версии 4.3.1 используется другой API для выплат
				var newPayout = new NewPayout
				{
					Amount = new Amount
					{
						Value = withdrawal.ActualAmount,
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
				Payout? yooKassaPayout = null;

				try
				{
					yooKassaPayout = _yooKassaClient.CreatePayout(newPayout);
				}
				catch (Exception ex)
				{
					_logger.LogError(ex, "YooKassa: Ошибка при создании выплаты - проверьте версию API и конфигурацию");

					// В версии 4.3.1 выплаты могут работать по-другому
					// Возвращаем ошибку с рекомендацией
					wallet.Balance += amount; // Вернуть средства
					return (false, null,
						"Выплаты не доступны в текущей конфигурации. " +
						"Проверьте документацию YooKassa для версии 4.3.1");
				}

				if (yooKassaPayout == null)
				{
					wallet.Balance += amount;
					return (false, null, "Не удалось создать выплату в YooKassa");
				}

				// Сохранить ID выплаты
				withdrawal.AggregatorWithdrawalId = yooKassaPayout.Id;
				withdrawal.MarkAsProcessing();

				// Добавить в БД
				await _withdrawalRepository.AddAsync(withdrawal, ct);

				_logger.LogInformation(
					$"YooKassa: Выплата создана - WithdrawalId: {withdrawal.Id}, " +
					$"YooKassaWithdrawalId: {yooKassaPayout.Id}, Status: {yooKassaPayout.Status}");

				return (true, withdrawal, null);
			}
			catch (Exception ex)
			{
				// Вернуть средства при ошибке
				wallet.Balance += amount;

				_logger.LogError(ex,
					"YooKassa: Ошибка при создании выплаты в версии 4.3.1");

				// Проверяем, что это за ошибка
				if (ex.Message.Contains("Recipient") || ex.Message.Contains("recipient"))
				{
					return (false, null,
						"API выплат изменился в версии 4.3.1. " +
						"Требуется обновить конфигурацию согласно документации YooKassa.");
				}

				return (false, null, ex.Message);
			}
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "YooKassa: Критическая ошибка при создании выплаты");
			return (false, null, "Внутренняя ошибка сервера");
		}
	}

	/// <summary>
	/// Получить выплату по ID
	/// </summary>
	public async Task<Result<WithdrawalDS, Error>> GetWithdrawalAsync(Guid withdrawalId, CancellationToken ct)
	{
		return await _withdrawalRepository.GetByIdAsync(withdrawalId, ct);
	}

	/// <summary>
	/// Получить выплату по ID YooKassa
	/// </summary>
	public async Task<Result<WithdrawalDS, Error>> GetWithdrawalByYooKassaIdAsync(string yooKassaWithdrawalId)
	{
		return await _withdrawalRepository.GetByAggregatorIdAsync(yooKassaWithdrawalId);
	}

	/// <summary>
	/// Обновить статус выплаты
	/// </summary>
	public async Task UpdateWithdrawalStatusAsync(Guid withdrawalId, WithdrawalStatus status, CancellationToken ct)
	{
		var withdrawalResult = await GetWithdrawalAsync(withdrawalId, ct);
		if (withdrawalResult.IsSuccess)
		{
			var withdrawal = withdrawalResult.Value;
			withdrawal.Status = status;
			withdrawal.UpdatedAt = DateTime.UtcNow;

			if (status == WithdrawalStatus.Succeeded)
			{
				withdrawal.MarkAsSucceeded();
			}

			await _withdrawalRepository.UpdateAsync(withdrawal, ct);
		}
	}

	/// <summary>
	/// Завершить выплату
	/// </summary>
	public async Task CompleteWithdrawalAsync(Guid withdrawalId, CancellationToken ct)
	{
		var withdrawalResult = await GetWithdrawalAsync(withdrawalId, ct);
		if (withdrawalResult.IsFailure)
			return;

		var withdrawal = withdrawalResult.Value;
		withdrawal.MarkAsSucceeded();
		await _withdrawalRepository.UpdateAsync(withdrawal, ct);

		_logger.LogInformation(
			$"YooKassa: Выплата завершена - WithdrawalId: {withdrawalId}, " +
			$"Amount: {withdrawal.ActualAmount}");
	}

	/// <summary>
	/// Отменить выплату и вернуть средства
	/// </summary>
	public async Task CancelWithdrawalAsync(Guid withdrawalId, string? reason = null, CancellationToken ct = default)
	{
		var withdrawalResult = await GetWithdrawalAsync(withdrawalId, ct);
		if (withdrawalResult.IsFailure)
			return;

		// Вернуть средства если выплата была в обработке
		var withdrawal = withdrawalResult.Value;
		var wallet = await _walletRepository.GetByIdAsync(withdrawal.WalletId, ct);
		if (wallet != null && withdrawal.Status == WithdrawalStatus.Processing)
		{
			wallet.Balance += withdrawal.RequestedAmount;
		}

		withdrawal.MarkAsCanceled(reason);
		await _withdrawalRepository.UpdateAsync(withdrawal, ct);

		_logger.LogInformation($"YooKassa: Выплата отменена - WithdrawalId: {withdrawalId}");
	}

	/// <summary>
	/// Получить выплаты пользователя
	/// </summary>
	public async Task<Result<List<WithdrawalDS>, Error>> GetUserWithdrawalsAsync(
		Guid userId,
		int skip = 0,
		int take = 10,
		CancellationToken ct = default)
	{
		return await _withdrawalRepository.GetUserWithdrawalsAsync(userId, skip, take, ct);
	}

	/// <summary>
	/// Маскировать номер карты
	/// </summary>
	private string MaskCardNumber(string cardNumber)
	{
		if (cardNumber.Length < 4)
			return "****";

		return $"{cardNumber.Substring(0, 4)}****{cardNumber.Substring(cardNumber.Length - 4)}";
	}
}