using System;
using System.Collections.Generic;
using System.Text;

namespace DigiStore.WalletService.Contracts.Responses;

public class TransactionResponse
{
	public Guid Id { get; set; }
	public Guid WalletId { get; set; }
	public decimal Amount { get; set; }
	public string Type { get; set; } = string.Empty;
	public string Status { get; set; } = string.Empty;
	public string Description { get; set; } = string.Empty;
	public decimal BalanceAfter { get; set; }
	public DateTime CreatedAt { get; set; }
}