using System;
using System.Collections.Generic;
using System.Text;

namespace DigiStore.WalletService.Contracts.Requests;

public record DepositRequest(decimal Amount, string? Description, string? PaymentMethod);
