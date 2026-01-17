using DigiStore.Enums;

namespace DigiStore.WalletService.Contracts.Responses;

public record WalletResponse(
	Guid Id,
	Guid UserId,
	decimal Balance,
	decimal TotalDeposited,
	decimal TotalWithdrawn,
	CurrencyCodes Currency,
	bool IsFrozen,
	DateTime CreatedAt,
	DateTime UpdatedAt
);
