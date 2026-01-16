namespace DigiStore.WalletService.Contracts.HttpClients;

public record WalletServiceOptions
{
	public string Url { get; init; } = string.Empty;

	public int TimeoutSeconds { get; init; } = 7;
}