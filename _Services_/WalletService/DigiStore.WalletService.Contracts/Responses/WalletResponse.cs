using System;
using System.Collections.Generic;
using System.Text;

namespace DigiStore.WalletService.Contracts.Responses;

public record WalletResponse(
	Guid Id,
	Guid UserId,
	decimal Balance,
	decimal TotalDeposited,
	decimal TotalWithdrawn,
	string Currency,
	bool IsFrozen,
	DateTime CreatedAt,
	DateTime UpdatedAt
);
