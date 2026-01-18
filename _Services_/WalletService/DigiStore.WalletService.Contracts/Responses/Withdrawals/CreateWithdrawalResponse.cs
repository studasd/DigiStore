using DigiStore.Enums;

namespace DigiStore.WalletService.Contracts.Responses.Withdrawals;

public record CreateWithdrawalResponse
(
	Guid WithdrawalId,
	decimal RequestedAmount,
	decimal Commission,
	decimal ActualAmount,
	string CardMask,
	WithdrawalStatus Status
	);
