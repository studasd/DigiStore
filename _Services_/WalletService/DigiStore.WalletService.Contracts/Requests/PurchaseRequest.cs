using System;
using System.Collections.Generic;
using System.Text;

namespace DigiStore.WalletService.Contracts.Requests;

public class PurchaseRequest
{
	public decimal Amount { get; set; }
	public string OrderId { get; set; }
	public string Description { get; set; }
}