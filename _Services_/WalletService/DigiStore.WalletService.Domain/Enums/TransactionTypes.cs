using System;
using System.Collections.Generic;
using System.Text;

namespace DigiStore.WalletService.Domain.Enums;

/// <summary>
/// Transaction type enum
/// </summary>
public enum TransactionTypes
{
	/// <summary>
	/// Money deposit (пополнение)
	/// </summary>
	Deposit = 1,
	/// <summary>
	/// Money withdrawal (снятие)
	/// </summary>
	Withdrawal = 2,
	/// <summary>
	/// Purchase (покупка товара)
	/// </summary>
	Purchase = 3,
	/// <summary>
	/// Refund (возврат)
	/// </summary>
	Refund = 4,
	/// <summary>
	/// Bonus/reward (бонус)
	/// </summary>
	Bonus = 5,
	/// <summary>
	/// Penalty/fine (штраф)
	/// </summary>
	Penalty = 6,
	/// <summary>
	/// Transfer between users (перевод)
	/// </summary>
	Transfer = 7,
	/// <summary>
	/// Adjustment (корректировка)
	/// </summary>
	Adjustment = 8
}