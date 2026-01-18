using DigiStore.Enums;

namespace DigiStore.WalletService.Contracts.Responses.Withdrawals;

public record WithdrawalResponse
(
	Guid WithdrawalId,
	decimal RequestedAmount,
	decimal Commission,
	decimal ActualAmount,
	string CardMask,
	WithdrawalStatus Status,
	DateTime CreatedAt,
	DateTime? CompletedAt
	);