namespace DigiStore.Enums;

/// <summary>
/// Статусы платежей YooKassa
/// </summary>
public enum PaymentStatus
{
	/// <summary>Платеж ожидает обработки</summary>
	Pending = 0,

	/// <summary>Платеж успешно выполнен</summary>
	Succeeded = 1,

	/// <summary>Платеж отменен</summary>
	Canceled = 2,

	/// <summary>Платеж возвращен</summary>
	Refunded = 3,

	/// <summary>Платеж создан</summary>
	Created = 4
}
