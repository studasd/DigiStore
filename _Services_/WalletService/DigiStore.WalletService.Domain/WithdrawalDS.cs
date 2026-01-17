using DigiStore.Enums;
using DigiStore.WalletService.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace DigiStore.WalletService.Domain;

/// <summary>
/// Модель выплаты (вывод с баланса)
/// </summary>
public class WithdrawalDS
{
	/// <summary>ID выплаты в системе</summary>
	public Guid Id { get; set; }

	/// <summary>ID кошелька</summary>
	public Guid WalletId { get; set; } = Guid.Empty;

	/// <summary>ID пользователя</summary>
	public Guid UserId { get; set; }

	public PaymentAggregators Aggregator { get; set; }

	/// <summary>ID выплаты в агрегаторе  YooKassa,FreeKassa...</summary>
	public string AggregatorWithdrawalId { get; set; } = string.Empty;

	/// <summary>Запрошенная сумма</summary>
	public decimal RequestedAmount { get; set; }

	/// <summary>Комиссия (5% по умолчанию)</summary>
	public decimal Commission { get; set; }

	/// <summary>Сумма после комиссии</summary>
	public decimal ActualAmount { get; set; }

	/// <summary>Валюта</summary>
	public CurrencyCodes Currency { get; set; }

	/// <summary>Статус выплаты</summary>
	public WithdrawalStatus Status { get; set; } = WithdrawalStatus.Pending;

	/// <summary>Описание</summary>
	public string Description { get; set; } = "Вывод средств";

	/// <summary>Маскированный номер карты</summary>
	public string? CardMask { get; set; }

	/// <summary>Сообщение об ошибке</summary>
	public string? ErrorMessage { get; set; }

	/// <summary>ID транзакции в кошельке</summary>
	public Guid? TransactionId { get; set; }

	/// <summary>Дата создания</summary>
	public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

	/// <summary>Дата последнего обновления</summary>
	public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

	/// <summary>Дата завершения</summary>
	public DateTime? CompletedAt { get; set; }


	// Navigation property
	public WalletDS? Wallet { get; set; }


	/// <summary>
	/// Создать новую выплату
	/// </summary>
	public static WithdrawalDS Create(Guid walletId, Guid userId, decimal amount)
	{
		if (amount <= 0)
			throw new InvalidOperationException("Сумма выплаты должна быть больше 0");

		// Расчет комиссии (5%)
		decimal commission = Math.Round(amount * 0.05m, 2);
		decimal actualAmount = amount - commission;

		return new WithdrawalDS
		{
			Id = Guid.NewGuid(),
			WalletId = walletId,
			UserId = userId,
			RequestedAmount = amount,
			Commission = commission,
			ActualAmount = actualAmount,
			Status = WithdrawalStatus.Pending,
			CreatedAt = DateTime.UtcNow
		};
	}

	/// <summary>Отметить выплату как успешную</summary>
	public void MarkAsSucceeded()
	{
		Status = WithdrawalStatus.Succeeded;
		CompletedAt = DateTime.UtcNow;
		UpdatedAt = DateTime.UtcNow;
	}

	/// <summary>Отметить выплату как обрабатываемую</summary>
	public void MarkAsProcessing()
	{
		Status = WithdrawalStatus.Processing;
		UpdatedAt = DateTime.UtcNow;
	}

	/// <summary>Отметить выплату как неудачную</summary>
	public void MarkAsFailed(string reason = "")
	{
		Status = WithdrawalStatus.Failed;
		ErrorMessage = reason;
		UpdatedAt = DateTime.UtcNow;
	}

	/// <summary>Отметить выплату как отмененную</summary>
	public void MarkAsCanceled(string? reason = null)
	{
		Status = WithdrawalStatus.Canceled;
		ErrorMessage = reason;
		UpdatedAt = DateTime.UtcNow;
	}

	/// <summary>Выплата успешна?</summary>
	public bool IsSucceeded => Status == WithdrawalStatus.Succeeded;

	/// <summary>Выплата в статусе Pending?</summary>
	public bool IsPending => Status == WithdrawalStatus.Pending;

	/// <summary>Выплата в процессе?</summary>
	public bool IsProcessing => Status == WithdrawalStatus.Processing;
}