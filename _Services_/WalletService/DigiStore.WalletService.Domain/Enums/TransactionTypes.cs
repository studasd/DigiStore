using System;
using System.Collections.Generic;
using System.Text;

namespace DigiStore.WalletService.Domain.Enums;

/// <summary>
/// Transaction type enum
/// </summary>
public enum TransactionTypes
{
	/// Пополнение
	Deposit = 1,

	/// Вывод с баланса
	Withdrawal = 2,

	/// покупка товара
	Purchase = 3,

	/// Возврат
	Refund = 4,

	/// Bonus/reward (бонус)
	Bonus = 5,

	/// Penalty/fine (штраф)
	Penalty = 6,

	/// Transfer between users (перевод)
	Transfer = 7,

	/// Adjustment (корректировка)
	Adjustment = 8
}
