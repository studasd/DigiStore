using DigiStore.Enums;

namespace DigiStore.TgBot.Application.DTOs;

public record TransactionDto(
	Guid Id,
	Guid WalletId,
	decimal Amount,
	TransactionTypes Type,
	TransactionStatuses Status,
	string Description,
	decimal BalanceAfter,
	DateTime CreatedAt);