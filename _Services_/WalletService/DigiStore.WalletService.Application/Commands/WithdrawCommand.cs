using System;
using System.Collections.Generic;
using System.Text;

namespace DigiStore.WalletService.Application.Commands;

public class WithdrawCommand
{
	public Guid UserId { get; set; }
	public decimal Amount { get; set; }
	public string Description { get; set; } = string.Empty;
	public string? ReferenceId { get; set; }
}