using DigiStore.Enums;

namespace DigiStore.UserService.Contracts.Requests;


/// <summary>
/// Request to create a new user (from Telegram or Web)
/// </summary>
public record CreateUserRequest
{
	/// <summary>
	/// User email (required)
	/// </summary>
	public string Email { get; set; } = string.Empty;

	/// <summary>
	/// User password (required for web, optional for Telegram)
	/// </summary>
	public string? Password { get; set; }

	/// <summary>
	/// First name
	/// </summary>
	public string? FirstName { get; set; }

	/// <summary>
	/// Last name
	/// </summary>
	public string? LastName { get; set; }

	/// <summary>
	/// Telegram user ID (optional, for linking)
	/// </summary>
	public long? TelegramId { get; set; }

	/// <summary>
	/// Phone number
	/// </summary>
	public string? PhoneNumber { get; set; }

	/// <summary>
	/// Language preference (default: 'en')
	/// </summary>
	public LanguageCodes LangCode { get; set; } = LanguageCodes.en;

	/// <summary>
	/// User source (Telegram, Web, etc.)
	/// </summary>
	public string Source { get; set; } = "Telegram";
}
