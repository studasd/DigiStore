using System;
using System.Collections.Generic;
using System.Text;

namespace DigiStore.Enums;

/// <summary>
/// Transaction status
/// </summary>
public enum TransactionStatuses
{
	/// <summary>Транзакция ожидает обработки</summary>
	Pending = 1,

	/// <summary>Транзакция успешно выполнена</summary>
	Completed = 2,

	/// <summary>Транзакция не выполнена</summary>
	Failed = 3,

	/// <summary>Транзакция отменена и возвращена</summary>
	Reversed = 4,

	/// <summary>Транзакция отменена пользователем</summary>
	Cancelled = 5
}