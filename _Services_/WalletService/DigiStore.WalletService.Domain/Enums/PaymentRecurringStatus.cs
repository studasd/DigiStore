namespace DigiStore.WalletService.Domain.Enums;

/// <summary>
/// Статусы рекуррентных платежей
/// </summary>
public enum PaymentRecurringStatus
{
	/// <summary>Подписка активна</summary>
	Active = 0,

	/// <summary>Подписка приостановлена</summary>
	Suspended = 1,

	/// <summary>Подписка отменена</summary>
	Canceled = 2,

	/// <summary>Подписка истекла</summary>
	Expired = 3
}