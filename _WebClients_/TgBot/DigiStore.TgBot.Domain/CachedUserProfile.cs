namespace DigiStore.TgBot.Domain;

/// <summary>
/// Cached user profile for session (to avoid multiple calls)
/// </summary>
public class CachedUserProfile
{
	public Guid UserId { get; set; }
	public long TelegramId { get; set; }
	public string Email { get; set; } = string.Empty;
	public string FirstName { get; set; } = string.Empty;
	public string LastName { get; set; } = string.Empty;
	public string? TelegramUsername { get; set; }
	public string LanguageCode { get; set; } = "en";
	public bool IsActive { get; set; }
	public List<string> Roles { get; set; } = new();
	public decimal Balance { get; set; }
	public string Currency { get; set; } = "RUB";
	public DateTime CreatedAt { get; set; }
	public DateTime UpdatedAt { get; set; }
	public string GetFullName() => $"{FirstName} {LastName}".Trim();
}
