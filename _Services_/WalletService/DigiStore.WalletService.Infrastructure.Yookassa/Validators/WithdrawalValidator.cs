using DigiStore.WalletService.Application.Configurations;
using System;
using System.Collections.Generic;
using System.Text;

namespace DigiStore.WalletService.Infrastructure.Yookassa.Validators;

/// <summary>
/// Валидатор выплат
/// </summary>
public class WithdrawalValidator
{
	private readonly YooKassaSettings _settings;

	public WithdrawalValidator(YooKassaSettings settings)
	{
		_settings = settings;
	}

	/// <summary>
	/// Валидировать сумму вывода
	/// </summary>
	public ValidationResultOLD ValidateWithdrawalAmount(decimal amount)
	{
		if (amount <= 0)
			return ValidationResultOLD.Failed("Сумма вывода должна быть больше 0");

		if (amount < _settings.MinWithdrawalAmount)
			return ValidationResultOLD.Failed(
				$"Минимальная сумма вывода: {_settings.MinWithdrawalAmount} руб.");

		if (amount > _settings.MaxWithdrawalAmount)
			return ValidationResultOLD.Failed(
				$"Максимальная сумма вывода: {_settings.MaxWithdrawalAmount} руб.");

		return ValidationResultOLD.Success();
	}

	/// <summary>
	/// Валидировать баланс кошелька
	/// </summary>
	public ValidationResultOLD ValidateBalance(decimal walletBalance, decimal withdrawalAmount)
	{
		if (walletBalance <= 0)
			return ValidationResultOLD.Failed("Баланс кошелька должен быть больше 0");

		if (walletBalance < withdrawalAmount)
			return ValidationResultOLD.Failed("Недостаточно средств для вывода");

		return ValidationResultOLD.Success();
	}

	/// <summary>
	/// Рассчитать сумму после комиссии
	/// </summary>
	public decimal CalculateAmountAfterCommission(decimal amount)
	{
		decimal commission = Math.Round(amount * (_settings.WithdrawalCommissionPercent / 100m), 2);
		return amount - commission;
	}

	/// <summary>
	/// Рассчитать размер комиссии
	/// </summary>
	public decimal CalculateCommission(decimal amount)
	{
		return Math.Round(amount * (_settings.WithdrawalCommissionPercent / 100m), 2);
	}

	/// <summary>
	/// Валидировать выплату полностью
	/// </summary>
	public ValidationResultOLD ValidateWithdrawal(Guid walletId, Guid userId, decimal walletBalance, decimal withdrawalAmount, string? cardNumber = null)
	{
		if (walletId == Guid.Empty)
			return ValidationResultOLD.Failed("WalletId не может быть пустым");

		if (userId == Guid.Empty)
			return ValidationResultOLD.Failed("UserId не может быть пустым");

		var amountValidation = ValidateWithdrawalAmount(withdrawalAmount);
		if (!amountValidation.IsValid)
			return amountValidation;

		var balanceValidation = ValidateBalance(walletBalance, withdrawalAmount);
		if (!balanceValidation.IsValid)
			return balanceValidation;

		if (string.IsNullOrEmpty(cardNumber))
			return ValidationResultOLD.Failed("Номер карты не может быть пустым");

		if (cardNumber.Length < 4)
			return ValidationResultOLD.Failed("Номер карты должен содержать минимум 4 цифры");

		return ValidationResultOLD.Success();
	}
}