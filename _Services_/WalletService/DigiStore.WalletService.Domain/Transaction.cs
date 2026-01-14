using DigiStore.WalletService.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace DigiStore.WalletService.Domain;

/// <summary>
/// Transaction - запись о пополнении/снятии
/// </summary>
public class Transaction
{
	public Guid Id { get; set; }
	public Guid WalletId { get; set; }
	public Guid UserId { get; set; }
	/// <summary>
	/// Transaction amount
	/// </summary>
	public decimal Amount { get; set; }
	/// <summary>
	/// Transaction type
	/// </summary>
	public TransactionTypes Type { get; set; }
	/// <summary>
	/// Status of transaction
	/// </summary>
	public TransactionStatuses Status { get; set; } = TransactionStatuses.Completed;
	/// <summary>
	/// Description (e.g., "Order #123", "Refund", etc.)
	/// </summary>
	public string Description { get; set; } = string.Empty;
	/// <summary>
	/// Reference to external service (OrderId, PaymentId, etc.)
	/// </summary>
	public string? ReferenceId { get; set; }
	/// <summary>
	/// Reference type (Order, Payment, Refund, etc.)
	/// </summary>
	public string? ReferenceType { get; set; }
	/// <summary>
	/// Balance after transaction
	/// </summary>
	public decimal BalanceAfter { get; set; }
	/// <summary>
	/// Payment method used
	/// </summary>
	public string? PaymentMethod { get; set; }
	/// <summary>
	/// Created timestamp
	/// </summary>
	public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
	/// <summary>
	/// Navigation to wallet
	/// </summary>
	public Wallet? Wallet { get; set; }
}