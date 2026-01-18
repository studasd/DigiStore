using DigiStore.Enums;

namespace DigiStore.WalletService.Contracts.Responses;

public record TransactionResponse(
	Guid Id, 
	Guid WalletId, 
	decimal Amount,
	TransactionTypes Type,
	TransactionStatuses Status, 
	string Description, 
	decimal BalanceAfter, 
	DateTime CreatedAt);