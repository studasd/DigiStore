using System;
using System.Collections.Generic;
using System.Text;

namespace DigiStore.WalletService.Contracts.Responses;

public class WalletResponse
{
	public Guid Id { get; set; }
	public Guid UserId { get; set; }
	public decimal Balance { get; set; }
	public decimal TotalDeposited { get; set; }
	public decimal TotalWithdrawn { get; set; }
	public string Currency { get; set; } = "RUB";
	public bool IsFrozen { get; set; }
	public DateTime CreatedAt { get; set; }
	public DateTime UpdatedAt { get; set; }
}
