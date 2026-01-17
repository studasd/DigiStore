namespace DigiStore.WalletService.Domain.Enums;

/// <summary>
/// Статусы выплат Агрегата
/// </summary>
public enum WithdrawalStatus
{
	/// <summary>Выплата в обработке</summary>
	Pending = 0,

	/// <summary>Выплата успешна</summary>
	Succeeded = 1,

	/// <summary>Выплата не удалась</summary>
	Failed = 2,

	/// <summary>Выплата в процессе</summary>
	Processing = 3,

	/// <summary>Выплата отменена</summary>
	Canceled = 4
}
