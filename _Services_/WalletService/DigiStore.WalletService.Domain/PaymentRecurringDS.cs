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
	public Guid Id { get; init; }

	/// <summary>ID кошелька</summary>
	public Guid WalletId { get; init; }

	/// <summary>ID пользователя</summary>
	public Guid UserId { get; init; }

	public PaymentAggregators Aggregator { get; init; }

	/// <summary>ID рекуррентного платежа в YooKassa</summary>
	public string AggregatorRecurringId { get; init; } = string.Empty;

	/// <summary>Сумма платежа</summary>
	public decimal Amount { get; init; }

	/// <summary>Валюта</summary>
	public CurrencyCodes Currency { get; init; }

	/// <summary>Интервал между платежами (в днях)</summary>
	public int IntervalDays { get; init; }

	/// <summary>Статус подписки</summary>
	public PaymentRecurringStatus Status { get; private set; } = PaymentRecurringStatus.Active;

	/// <summary>Описание подписки</summary>
	public string Description { get; init; } = string.Empty;

	/// <summary>ID платежного средства</summary>
	public string? PaymentInstrumentId { get; init; }

	/// <summary>Количество успешных платежей</summary>
	public int SuccessfulPayments { get; private set; }

	/// <summary>Количество неудачных платежей</summary>
	public int FailedPayments { get; private set; }

	/// <summary>Дата следующего платежа</summary>
	public DateTime NextPaymentDate { get; private set; }

	/// <summary>Дата последнего платежа</summary>
	public DateTime? LastPaymentDate { get; private set; }

	/// <summary>Дата создания</summary>
	public DateTime CreatedAt { get; init; } = DateTime.UtcNow;

	/// <summary>Дата последнего обновления</summary>
	public DateTime UpdatedAt { get; private set; } = DateTime.UtcNow;

	/// <summary>Дата отмены</summary>
	public DateTime? CancelledAt { get; private set; }

	// Navigation properties
	public WalletDS? Wallet { get; init; }
	public ICollection<PaymentDS> Payments { get; init; } = new List<PaymentDS>();



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