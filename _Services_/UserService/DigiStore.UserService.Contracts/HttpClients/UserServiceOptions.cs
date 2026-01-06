namespace DigiStore.UserService.Contracts.HttpClients;

public record UserServiceOptions
{
	public string Url { get; init; } = string.Empty;

	public int TimeoutSeconds { get; init; } = 7;
}