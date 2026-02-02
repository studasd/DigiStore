using CSharpFunctionalExtensions;
using StudCoreKit.SharedKernel;
using DigiStore.WalletService.Application.Configurations;
using Microsoft.Extensions.Options;

namespace DigiStore.WalletService.Application.Validators;

/// <summary>
/// Валидатор платежей
/// </summary>
public class PaymentValidator
{
	private readonly YooKassaSettings _settings;

	public PaymentValidator(IOptions<YooKassaSettings> settings)
	{
		_settings = settings.Value;
	}


	/// <summary>
	/// Валидировать сумму пополнения
	/// </summary>
	public UnitResult<Error> ValidateDepositAmount(decimal amount)
	{
		if (amount <= 0)
			return Error.Validation("amount.fail", "Сумма пополнения должна быть больше 0");

		if (amount < _settings.MinDepositAmount)
			return Error.Validation("amount.fail", $"Минимальная сумма пополнения: {_settings.MinDepositAmount} руб.");

		if (amount > _settings.MaxDepositAmount)
			return Error.Validation("amount.fail", $"Максимальная сумма пополнения: {_settings.MaxDepositAmount} руб.");

		return Result.Success<Error>();
	}


	/// <summary>
	/// Валидировать платеж полностью
	/// </summary>
	public UnitResult<Error> ValidatePayment(Guid walletId, Guid userId, decimal amount, string description = "")
	{
		if (walletId == Guid.Empty)
			return Error.Validation("payment.fail", "WalletId не может быть пустым");

		if (userId == Guid.Empty)
			return Error.Validation("payment.fail", "UserId не может быть пустым");

		var amountValidation = ValidateDepositAmount(amount);
		if (amountValidation.IsFailure)
			return amountValidation;

		if (!string.IsNullOrEmpty(description) && description.Length > 500)
			return Error.Validation("payment.fail", "Описание не может быть длиннее 500 символов");

		return Result.Success<Error>();
	}
}