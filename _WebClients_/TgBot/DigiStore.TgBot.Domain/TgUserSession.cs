using DigiStore.TgBot.Domain.ValueObjects;
using DigiStore.UserService.Contracts.Enums;

namespace DigiStore.TgBot.Domain;

public class TgUserSession
{
    public Guid Id { get; set; } = Guid.NewGuid();

	public Guid UserId { get; set; }

	public long TelegramId { get; set; }

    public string CurrentState { get; set; } = string.Empty;

    public LanguageCodes LangCode { get; set; } = LanguageCodes.en;

	public CachedUserProfileVO? CachedProfile { get; set; }

	public string? PendingTopUpAggregator { get; set; }
	public decimal? PendingTopUpAmount { get; set; }
	public long? PendingTopUpChatId { get; set; }
	public int? PendingTopUpMessageId { get; set; }

    public DateTime LastActivity { get; set; }

    public DateTime CreatedAt { get; set; }


	public void UpdateActivity()
	{
		LastActivity = DateTime.UtcNow;
	}

	public void SetState(string state)
	{
		CurrentState = state;
		UpdateActivity();
	}

}
