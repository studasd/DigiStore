using DigiStore.Enums;
using DigiStore.TgBot.Domain.ValueObjects;

namespace DigiStore.TgBot.Domain;

public class TgSession
{
    public Guid Id { get; init; } = Guid.NewGuid();

	public Guid UserId { get; set; }

	public long TelegramId { get; init; }

    public string CurrentState { get; set; } = string.Empty;

    public LanguageCodes LangCode { get; set; } = LanguageCodes.en;

	public CachedUserProfileVO? CachedProfile { get; set; }

	// Per-message interaction contexts (allows parallel flows bound to concrete bot messages).
	// Key format: "{chatId}:{messageId}".
	public Dictionary<string, MessageContextVO> MessageContexts { get; set; } = new();

	// Multiple pending payments support.
	// Key: WalletService PaymentId. Value: info required to edit the exact telegram message that contained the payment link.
	public Dictionary<Guid, PendingPaymentMessageVO> PendingPayments { get; set; } = new();

    public DateTime LastActivity { get; set; }

    public DateTime CreatedAt { get; init; }



	public void UpdateActivity()
	{
		LastActivity = DateTime.UtcNow;
	}

	public void SetState(string state)
	{
		CurrentState = state;
		UpdateActivity();
	}

	public static string BuildMessageContextKey(long chatId, int messageId) => $"{chatId}:{messageId}";

	public MessageContextVO? GetMessageContext(long chatId, int messageId)
	{
		var key = BuildMessageContextKey(chatId, messageId);
		return MessageContexts.TryGetValue(key, out var ctx) ? ctx : null;
	}

	public void UpsertMessageContext(long chatId, int messageId, MessageContextVO ctx)
	{
		var key = BuildMessageContextKey(chatId, messageId);
		MessageContexts[key] = ctx;
		UpdateActivity();
	}

	public void RemoveMessageContext(long chatId, int messageId)
	{
		var key = BuildMessageContextKey(chatId, messageId);
		MessageContexts.Remove(key);
		UpdateActivity();
	}

}
