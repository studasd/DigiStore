namespace DigiStore.TgBot.Domain.ValueObjects;

public record MessageContextVO(
	string State,
	PendingTopUpVO? PendingTopUp,
	DateTime UpdatedAtUtc);


public record MessageContextsVO
{
	public Dictionary<string, MessageContextVO> MessageContexts {  get; set; }
}