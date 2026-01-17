using DigiStore.Enums;

namespace DigiStore.WalletService.Contracts.Responses.Payments;

public record CreatePaymentResponse(Guid PaymentId, string RredirectUrl, decimal Amount, PaymentStatus Status);
