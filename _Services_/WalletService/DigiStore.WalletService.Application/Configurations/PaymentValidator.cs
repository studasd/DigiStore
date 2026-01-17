using CSharpFunctionalExtensions;
using DigiStore.SharedKernel;

namespace DigiStore.WalletService.Application.Configurations;

/// <summary>
/// Валидатор платежей
/// </summary>
public class PaymentValidator
{
	private readonly YooKassaSettings _settings;

	public PaymentValidator(YooKassaSettings settings)
	{
		_settings = settings;
	}


	/// <summary>
	/// Валидировать сумму пополнения
	/// </summary>
	public UnitResult<Error> ValidateDepositAmount(decimal amount)
	{
		if (amount <= 0)
			return Error.Failure("amount.fail", "Сумма пополнения должна быть больше 0");

		if (amount < _settings.MinDepositAmount)
			return Error.Failure("amount.fail", $"Минимальная сумма пополнения: {_settings.MinDepositAmount} руб.");

		if (amount > _settings.MaxDepositAmount)
			return Error.Failure("amount.fail", $"Максимальная сумма пополнения: {_settings.MaxDepositAmount} руб.");

		return Result.Success<Error>();
	}



	/// <summary>
	/// Валидировать сумму пополнения
	/// </summary>
	public ValidationResultOLD ValidateDepositAmountOLD(decimal amount)
	{
		if (amount <= 0)
			return ValidationResultOLD.Failed("Сумма пополнения должна быть больше 0");

		if (amount < _settings.MinDepositAmount)
			return ValidationResultOLD.Failed(
				$"Минимальная сумма пополнения: {_settings.MinDepositAmount} руб.");

		if (amount > _settings.MaxDepositAmount)
			return ValidationResultOLD.Failed(
				$"Максимальная сумма пополнения: {_settings.MaxDepositAmount} руб.");

		return ValidationResultOLD.Success();
	}

	/// <summary>
	/// Валидировать платеж полностью
	/// </summary>
	public ValidationResultOLD ValidatePayment(Guid walletId, Guid userId, decimal amount, string description = "")
	{
		if (walletId == Guid.Empty)
			return ValidationResultOLD.Failed("WalletId не может быть пустым");

		if (userId == Guid.Empty)
			return ValidationResultOLD.Failed("UserId не может быть пустым");

		var amountValidation = ValidateDepositAmountOLD(amount);
		if (!amountValidation.IsValid)
			return amountValidation;

		if (!string.IsNullOrEmpty(description) && description.Length > 500)
			return ValidationResultOLD.Failed("Описание не может быть длиннее 500 символов");

		return ValidationResultOLD.Success();
	}
}

/// <summary>
/// Результат валидации
/// </summary>
public class ValidationResultOLD
{
	/// <summary>Валидация прошла успешно?</summary>
	public bool IsValid { get; set; }

	/// <summary>Сообщение об ошибке (если не прошла)</summary>
	public string? ErrorMessage { get; set; }

	/// <summary>Успешный результат</summary>
	public static ValidationResultOLD Success() => new() { IsValid = true };

	/// <summary>Результат с ошибкой</summary>
	public static ValidationResultOLD Failed(string message) => new() { IsValid = false, ErrorMessage = message };
}