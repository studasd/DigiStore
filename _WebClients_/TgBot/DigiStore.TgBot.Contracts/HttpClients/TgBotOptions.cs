namespace DigiStore.TgBot.Contracts.HttpClients;

public record TgBotOptions
{
	public string Url { get; init; } = string.Empty;

	public int TimeoutSeconds { get; init; } = 7;
}