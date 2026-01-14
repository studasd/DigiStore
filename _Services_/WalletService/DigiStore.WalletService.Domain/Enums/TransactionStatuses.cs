using System;
using System.Collections.Generic;
using System.Text;

namespace DigiStore.WalletService.Domain.Enums;

/// <summary>
/// Transaction status
/// </summary>
public enum TransactionStatuses
{
	Pending = 1,
	Completed = 2,
	Failed = 3,
	Reversed = 4,
	Cancelled = 5
}