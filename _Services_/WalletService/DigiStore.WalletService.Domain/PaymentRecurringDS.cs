using DigiStore.Enums;
using DigiStore.WalletService.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace DigiStore.WalletService.Domain;

/// <summary>
/// Модель рекуррентного платежа (подписка)
/// </summary>
public class PaymentRecurringDS
{
	/// <summary>ID подписки в системе</summary>
	public Guid Id { get; set; }

	/// <summary>ID кошелька</summary>
	public Guid WalletId { get; set; }

	/// <summary>ID пользователя</summary>
	public Guid UserId { get; set; }

	public PaymentAggregators Aggregator { get; set; }

	/// <summary>ID рекуррентного платежа в YooKassa</summary>
	public string AggregatorRecurringId { get; set; } = string.Empty;

	/// <summary>Сумма платежа</summary>
	public decimal Amount { get; set; }

	/// <summary>Валюта</summary>
	public CurrencyCodes Currency { get; set; }

	/// <summary>Интервал между платежами (в днях)</summary>
	public int IntervalDays { get; set; }

	/// <summary>Статус подписки</summary>
	public PaymentRecurringStatus Status { get; set; } = PaymentRecurringStatus.Active;

	/// <summary>Описание подписки</summary>
	public string Description { get; set; } = string.Empty;

	/// <summary>ID платежного средства</summary>
	public string? PaymentInstrumentId { get; set; }

	/// <summary>Количество успешных платежей</summary>
	public int SuccessfulPayments { get; set; }

	/// <summary>Количество неудачных платежей</summary>
	public int FailedPayments { get; set; }

	/// <summary>Дата следующего платежа</summary>
	public DateTime NextPaymentDate { get; set; }

	/// <summary>Дата последнего платежа</summary>
	public DateTime? LastPaymentDate { get; set; }

	/// <summary>Дата создания</summary>
	public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

	/// <summary>Дата последнего обновления</summary>
	public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

	/// <summary>Дата отмены</summary>
	public DateTime? CancelledAt { get; set; }

	// Navigation properties
	public WalletDS? Wallet { get; set; }
	public ICollection<PaymentDS> Payments { get; set; } = new List<PaymentDS>();



	/// <summary>
	/// Создать новую подписку
	/// </summary>
	public static PaymentRecurringDS Create(
		Guid walletId,
		Guid userId,
		decimal amount,
		int intervalDays,
		string description = "")
	{
		if (amount <= 0)
			throw new InvalidOperationException("Сумма должна быть больше 0");
		if (intervalDays <= 0)
			throw new InvalidOperationException("Интервал должен быть больше 0");

		return new PaymentRecurringDS
		{
			Id = Guid.NewGuid(),
			WalletId = walletId,
			UserId = userId,
			Amount = amount,
			IntervalDays = intervalDays,
			Description = description,
			NextPaymentDate = DateTime.UtcNow.AddDays(intervalDays),
			Status = PaymentRecurringStatus.Active,
			CreatedAt = DateTime.UtcNow
		};
	}

	/// <summary>Активировать подписку</summary>
	public void Activate()
	{
		Status = PaymentRecurringStatus.Active;
		UpdatedAt = DateTime.UtcNow;
	}

	/// <summary>Приостановить подписку</summary>
	public void Suspend()
	{
		Status = PaymentRecurringStatus.Suspended;
		UpdatedAt = DateTime.UtcNow;
	}

	/// <summary>Отменить подписку</summary>
	public void Cancel()
	{
		Status = PaymentRecurringStatus.Canceled;
		CancelledAt = DateTime.UtcNow;
		UpdatedAt = DateTime.UtcNow;
	}

	/// <summary>Записать успешный платеж</summary>
	public void RecordSuccessfulPayment()
	{
		SuccessfulPayments++;
		LastPaymentDate = DateTime.UtcNow;
		NextPaymentDate = DateTime.UtcNow.AddDays(IntervalDays);
		UpdatedAt = DateTime.UtcNow;
	}

	/// <summary>Записать неудачный платеж</summary>
	public void RecordFailedPayment()
	{
		FailedPayments++;
		NextPaymentDate = DateTime.UtcNow.AddDays(IntervalDays);
		UpdatedAt = DateTime.UtcNow;
	}

	/// <summary>Подписка активна?</summary>
	public bool IsActive => Status == PaymentRecurringStatus.Active;

	/// <summary>Пора выполнять следующий платеж?</summary>
	public bool IsTimeForNextPayment => IsActive && NextPaymentDate <= DateTime.UtcNow;
}