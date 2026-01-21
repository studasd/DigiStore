using DigiStore.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace DigiStore.WalletService.Domain;

/// <summary>
/// Transaction - запись о пополнении/снятии баланса кошелька
/// </summary>
public class TransactionDS
{
	public Guid Id { get; init; }
	public Guid WalletId { get; init; }
	public Guid UserId { get; init; }

	/// <summary>
	/// Transaction amount
	/// </summary>
	public decimal Amount { get; init; }

	/// <summary>
	/// Transaction type
	/// </summary>
	public TransactionTypes Type { get; init; }

	/// <summary>
	/// Status of transaction
	/// </summary>
	public TransactionStatuses Status { get; init; } = TransactionStatuses.Completed;

	/// <summary>
	/// Description (e.g., "Order #123", "Refund", etc.)
	/// </summary>
	public string Description { get; init; } = string.Empty;

	/// <summary>
	/// Reference to external service (OrderId, PaymentId, etc.)
	/// </summary>
	public string? ReferenceId { get; init; }

	/// <summary>
	/// Reference type (Order, Payment, Refund, etc.)
	/// </summary>
	public string? ReferenceType { get; init; }

	/// <summary>
	/// Balance after transaction
	/// </summary>
	public decimal BalanceAfter { get; init; }

	/// <summary>
	/// Payment method used
	/// </summary>
	public string? PaymentMethod { get; init; }

	/// <summary>
	/// Created timestamp
	/// </summary>
	public DateTime CreatedAt { get; init; } = DateTime.UtcNow;


	/// <summary>
	/// Navigation to wallet
	/// </summary>
	public WalletDS? Wallet { get; init; }
}