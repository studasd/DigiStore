using CSharpFunctionalExtensions;
using DigiStore.SharedKernel;
using DigiStore.WalletService.Application.Interfaces;
using DigiStore.WalletService.Application.Validators;
using DigiStore.WalletService.Domain;
using DigiStore.WalletService.Domain.Enums;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace DigiStore.WalletService.Application.Services;

/// <summary>
/// Сервис управления выплатами YooKassa
/// Обновлено для версии 4.3.1
/// </summary>
public class WithdrawalService : IWithdrawalService
{
	private readonly WithdrawalValidator _validator;
    private readonly IYookassaProvider _yookassaProvider;
    private readonly IWalletRepository _walletRepository;
	private readonly IWithdrawalRepository _withdrawalRepository;
	private readonly ILogger<WithdrawalService> _logger;

	public WithdrawalService(
		WithdrawalValidator validator,
		IYookassaProvider yookassaProvider,
		IWalletRepository walletRepository,
		IWithdrawalRepository withdrawalRepository,
		ILogger<WithdrawalService> logger)
	{
		_validator = validator;
        _yookassaProvider = yookassaProvider;
        _walletRepository = walletRepository;
		_withdrawalRepository = withdrawalRepository;
		_logger = logger;
	}

	/// <summary>
	/// Создать выплату на карту
	/// </summary>
	public async Task<Result<WithdrawalDS, Error>> CreateWithdrawalAsync(
		Guid walletId,
		Guid userId,
		decimal amount,
		string cardNumber,
		CancellationToken token)
	{
		_logger.LogInformation($"YooKassa: Создание выплаты - WalletId: {walletId}, Amount: {amount}");

		// Проверить кошелек
		var walletResult = await _walletRepository.GetByIdAsync(walletId, token);
		if (walletResult.IsFailure)
			return walletResult.Error;

		var wallet = walletResult.Value;

		// Валидировать выплату
		var validation = _validator.ValidateWithdrawal(walletId, userId, wallet.Balance, amount, cardNumber);
		if (validation.IsFailure)
		{
			_logger.LogWarning($"YooKassa: Ошибка валидации - {validation.Error.GetMessage()}");
			return validation.Error;
		}

		// Создать локальную выплату
		var withdrawal = WithdrawalDS.Create(walletId, userId, amount);
		withdrawal.CardMask = MaskCardNumber(cardNumber);

		// Создать выплату в YooKassa
		var createWithdrawalResult = await _yookassaProvider.CreateWithdrawalAsync(walletId, withdrawal.Id, amount, withdrawal.ActualAmount, token);
		if (createWithdrawalResult.IsFailure)
			return createWithdrawalResult.Error;

		// Вычесть сумму со счета
		wallet.Balance -= amount;
		

		// Сохранить ID выплаты
		withdrawal.AggregatorWithdrawalId = createWithdrawalResult.Value;
		withdrawal.MarkAsProcessing();

		// Добавить в БД
		var withdrawalAddResult = await _withdrawalRepository.AddAsync(withdrawal, token);

		if (withdrawalAddResult.IsFailure)
			return withdrawalAddResult.Error;

		return withdrawal;
			
	}


	/// <summary>
	/// Отменить выплату и вернуть средства
	/// </summary>
	public async Task<UnitResult<Error>> CancelWithdrawalAsync(Guid withdrawalId, string? reason = null, CancellationToken token = default)
	{
		var withdrawalResult = await _withdrawalRepository.GetByIdAsync(withdrawalId, token);
		if (withdrawalResult.IsFailure)
			return withdrawalResult.Error;

		// Вернуть средства если выплата была в обработке
		var withdrawal = withdrawalResult.Value;
		if (withdrawal.Status != WithdrawalStatus.Processing)
			return Error.Failure("cancel.withdrawal.status.bad", "Only withdrawals in processing status can be canceled.");


		var walletResult = await _walletRepository.GetByIdAsync(withdrawal.WalletId, token);

		if(walletResult.IsFailure)
			return walletResult.Error;

		var wallet = walletResult.Value;
			
		wallet.Balance += withdrawal.RequestedAmount;

		withdrawal.MarkAsCanceled(reason);
		var updateResult = await _withdrawalRepository.UpdateAsync(withdrawal, token);
		if(updateResult.IsFailure)
			return updateResult.Error;

		var updateWalletResult = await _walletRepository.UpdateAsync(wallet, token);
		if (updateWalletResult.IsFailure)
			return updateWalletResult.Error;

		_logger.LogInformation($"YooKassa: Выплата отменена - WithdrawalId: {withdrawalId}");
		return Result.Success<Error>();
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