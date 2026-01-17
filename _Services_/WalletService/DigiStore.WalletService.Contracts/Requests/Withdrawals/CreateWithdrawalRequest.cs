using System;
using System.Collections.Generic;
using System.Text;

namespace DigiStore.WalletService.Contracts.Requests.Withdrawals;

public record CreateWithdrawalRequest(Guid WalletId, decimal Amount, string CardNumber);