using DigiStore.Enums;

namespace DigiStore.TgBot.Domain.ValueObjects;

/// <summary>
/// Cached user profile for session (to avoid multiple calls)
/// </summary>
public class CachedUserProfileVO
{
	public Guid UserId { get; set; }
	public long TelegramId { get; set; }
	public string Email { get; set; } = string.Empty;
	public string FirstName { get; set; } = string.Empty;
	public string LastName { get; set; } = string.Empty;
	public string? Username { get; set; }
	public LanguageCodes LangCode { get; set; } = LanguageCodes.en;
	public bool IsActive { get; set; }
	public List<string> Roles { get; set; } = new();
	public decimal Balance { get; set; }
	public string Currency { get; set; } = "RUB";
	public DateTime CreatedAt { get; set; }
	public DateTime UpdatedAt { get; set; }

	public string GetFullName() => $"{FirstName} {LastName}".Trim();
}
