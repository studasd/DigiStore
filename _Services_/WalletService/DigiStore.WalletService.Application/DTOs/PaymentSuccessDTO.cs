namespace DigiStore.WalletService.Application.DTOs;

public record PaymentSuccessDTO(string AggregatorPaymentId, decimal Amount, string PaymentMethod, PaymentMetaDTO? MetaData = null);

public record PaymentMetaDTO(Guid WalletId, Guid UserId, Guid PaymentId);
