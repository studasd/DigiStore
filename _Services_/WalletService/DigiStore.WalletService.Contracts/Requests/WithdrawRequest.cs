namespace DigiStore.WalletService.Contracts.Requests;

public class WithdrawRequest
{
	public decimal Amount { get; set; }
	public string? Description { get; set; }
}