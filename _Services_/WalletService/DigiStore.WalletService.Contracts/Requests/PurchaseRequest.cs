using System;
using System.Collections.Generic;
using System.Text;

namespace DigiStore.WalletService.Contracts.Requests;

public record PurchaseRequest(decimal Amount, string OrderId, string Description);
