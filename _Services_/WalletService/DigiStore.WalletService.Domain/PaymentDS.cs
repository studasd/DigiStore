using DigiStore.Enums;

namespace DigiStore.WalletService.Domain;

/// <summary>
/// Модель платежа (пополнение баланса)
/// </summary>
public class PaymentDS
{
	/// <summary>ID платежа в системе</summary>
	public Guid Id { get; set; }

	/// <summary>ID кошелька</summary>
	public Guid WalletId { get; set; }

	/// <summary>ID пользователя</summary>
	public Guid UserId { get; set; }

	public PaymentAggregators Aggregator { get; set; }

	/// <summary>ID платежа у агрегатора YooKassa,FreeKassa... </summary>
	public string AggregatorPaymentId { get; set; } = string.Empty;

	/// <summary>Сумма платежа</summary>
	public decimal Amount { get; set; }

	/// <summary>Валюта</summary>
	public CurrencyCodes Currency { get; set; }

	/// <summary>Статус платежа</summary>
	public PaymentStatus Status { get; set; } = PaymentStatus.Pending;

	/// <summary>Описание платежа</summary>
	public string Description { get; set; } = string.Empty;

	/// <summary>Метод оплаты (card, wallet, etc)</summary>
	public string? PaymentMethod { get; set; }

	/// <summary>ID рекуррентного платежа (если есть)</summary>
	public Guid? RecurringPaymentId { get; set; }

	/// <summary>URL возврата</summary>
	public string? ReturnUrl { get; set; }

	/// <summary>Сообщение об ошибке</summary>
	public string? ErrorMessage { get; set; }

	/// <summary>ID транзакции в кошельке</summary>
	public Guid? TransactionId { get; set; }

	/// <summary>Дата создания</summary>
	public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

	/// <summary>Дата последнего обновления</summary>
	public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

	/// <summary>Дата подтверждения</summary>
	public DateTime? ConfirmedAt { get; set; }

	// Navigation properties
	public WalletDS? Wallet { get; set; }
	public PaymentRecurringDS? RecurringPayment { get; set; }



	/// <summary>
	/// Создать новый платеж
	/// </summary>
	public static PaymentDS Create(Guid walletId, Guid userId, decimal amount, PaymentAggregators aggregator, string description, string returnUrl)
	{
		if (amount <= 0)
			throw new InvalidOperationException("Сумма платежа должна быть больше 0");

		return new PaymentDS
		{
			Id = Guid.NewGuid(),
			WalletId = walletId,
			UserId = userId,
			Amount = amount,
			Description = description,
			Aggregator = aggregator,
			ReturnUrl = returnUrl,
			Status = PaymentStatus.Created,
			CreatedAt = DateTime.UtcNow
		};
	}

	/// <summary>Отметить платеж как успешный</summary>
	public void MarkAsSucceeded(string paymentMethodType = "")
	{
		Status = PaymentStatus.Succeeded;
		ConfirmedAt = DateTime.UtcNow;
		UpdatedAt = DateTime.UtcNow;
		PaymentMethod = paymentMethodType;
	}

	/// <summary>Отметить платеж как отмененный</summary>
	public void MarkAsCanceled(string? reason = null)
	{
		Status = PaymentStatus.Canceled;
		ErrorMessage = reason;
		UpdatedAt = DateTime.UtcNow;
	}

	/// <summary>Установить ошибку платежа</summary>
	public void SetError(string errorMessage)
	{
		Status = PaymentStatus.Canceled;
		ErrorMessage = errorMessage;
		UpdatedAt = DateTime.UtcNow;
	}

	/// <summary>Платеж успешен?</summary>
	public bool IsSucceeded => Status == PaymentStatus.Succeeded;

	/// <summary>Платеж в статусе Pending?</summary>
	public bool IsPending => Status == PaymentStatus.Pending;
}