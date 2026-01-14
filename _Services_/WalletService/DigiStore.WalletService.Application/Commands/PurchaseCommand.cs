using System;
using System.Collections.Generic;
using System.Text;

namespace DigiStore.WalletService.Application.Commands;

public class PurchaseCommand
{
	public Guid UserId { get; set; }
	public decimal Amount { get; set; }
	public string OrderId { get; set; } = string.Empty;
	public string Description { get; set; } = string.Empty;
}