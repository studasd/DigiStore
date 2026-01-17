using DigiStore.Enums;

namespace DigiStore.WalletService.Contracts.Responses.Payments;

public record PaymentResponse(
	Guid PaymentId, 
	decimal Amount,
	PaymentStatus Status, 
	string Description, 
	DateTime CreatedAt, 
	DateTime? ConfirmedAt
	);
