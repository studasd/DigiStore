namespace DigiStore.TgBot.Contracts.Requests;

public record CancelPaymentRequest(Guid PaymentId, string? Reason = null);
