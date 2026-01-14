using System;
using System.Collections.Generic;
using System.Text;

namespace DigiStore.WalletService.Application.Commands;

public class DepositCommand
{
	public Guid UserId { get; set; }
	public decimal Amount { get; set; }
	public string Description { get; set; } = string.Empty;
	public string? PaymentMethod { get; set; }
	public string? ReferenceId { get; set; }
}