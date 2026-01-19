using DigiStore.Enums;

namespace DigiStore.WalletService.Contracts.Requests.Payments;

public record CreatePaymentRequest(PaymentAggregators Aggregator, decimal Amount, string Description, string ReturnUrl);