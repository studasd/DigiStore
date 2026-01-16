using System;
using System.Collections.Generic;
using System.Text;

namespace DigiStore.WalletService.Contracts.Responses;

public record TransactionResponse(
	Guid Id, 
	Guid WalletId, 
	decimal Amount, 
	string Type, 
	string Status, 
	string Description, 
	decimal BalanceAfter, 
	DateTime CreatedAt);