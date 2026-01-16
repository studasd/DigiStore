namespace DigiStore.WalletService.Contracts.Requests;

public record WithdrawRequest(decimal Amount, string? Description);
