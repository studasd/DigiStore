using CSharpFunctionalExtensions;
using StudCoreKit.SharedKernel;
using DigiStore.WalletService.Application.Configurations;
using Microsoft.Extensions.Options;

namespace DigiStore.WalletService.Application.Validators;

/// <summary>
/// Валидатор выплат
/// </summary>
public class WithdrawalValidator
{
	private readonly YooKassaSettings _settings;

	public WithdrawalValidator(IOptions<YooKassaSettings> settings)
	{
		_settings = settings.Value;
	}

	/// <summary>
	/// Валидировать сумму вывода
	/// </summary>
	public UnitResult<Error> ValidateWithdrawalAmount(decimal amount)
	{
		if (amount <= 0)
			return Error.Validation("withdrawal.amount", "Сумма вывода должна быть больше 0");

		if (amount < _settings.MinWithdrawalAmount)
			return Error.Validation("withdrawal.amount", $"Минимальная сумма вывода: {_settings.MinWithdrawalAmount} руб.");

		if (amount > _settings.MaxWithdrawalAmount)
			return Error.Validation("withdrawal.amount", $"Максимальная сумма вывода: {_settings.MaxWithdrawalAmount} руб.");

		return Result.Success<Error>();
	}

	/// <summary>
	/// Валидировать баланс кошелька
	/// </summary>
	public UnitResult<Error> ValidateBalance(decimal walletBalance, decimal withdrawalAmount)
	{
		if (walletBalance <= 0)
			return Error.Validation("validate.balance", "Баланс кошелька должен быть больше 0");

		if (walletBalance < withdrawalAmount)
			return Error.Validation("validate.balance", "Недостаточно средств для вывода");

		return Result.Success<Error>();
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
	public UnitResult<Error> ValidateWithdrawal(Guid walletId, Guid userId, decimal walletBalance, decimal withdrawalAmount, string? cardNumber = null)
	{
		if (walletId == Guid.Empty)
			return Error.Validation("validate.withdrawal", "WalletId не может быть пустым");

		if (userId == Guid.Empty)
			return Error.Validation("validate.withdrawal", "UserId не может быть пустым");

		var amountValidation = ValidateWithdrawalAmount(withdrawalAmount);
		if (amountValidation.IsFailure)
			return amountValidation;

		var balanceValidation = ValidateBalance(walletBalance, withdrawalAmount);
		if (balanceValidation.IsFailure)
			return balanceValidation;

		if (string.IsNullOrEmpty(cardNumber))
			return Error.Validation("validate.withdrawal", "Номер карты не может быть пустым");

		if (cardNumber.Length < 4)
			return Error.Validation("validate.withdrawal", "Номер карты должен содержать минимум 4 цифры");

		return Result.Success<Error>();

	}
}