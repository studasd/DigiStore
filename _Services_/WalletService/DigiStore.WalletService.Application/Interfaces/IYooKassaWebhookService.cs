using System;
using System.Collections.Generic;
using System.Text;

namespace DigiStore.WalletService.Application.Interfaces;

public interface IYooKassaWebhookService
{
	bool VerifyWebhookSignature(string jsonBody, string signatureHeader);

	Task ProcessWebhookAsync(string jsonBody, CancellationToken ct);
}
