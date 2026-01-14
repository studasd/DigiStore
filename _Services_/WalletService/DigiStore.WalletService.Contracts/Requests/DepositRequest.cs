using System;
using System.Collections.Generic;
using System.Text;

namespace DigiStore.WalletService.Contracts.Requests;

public class DepositRequest
{
	public decimal Amount { get; set; }
	public string? Description { get; set; }
	public string? PaymentMethod { get; set; }
}
