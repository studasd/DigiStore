using DigiStore.Enums;

namespace DigiStore.UserService.Contracts.Responses;


/// <summary>
/// Response DTO for user profile
/// </summary>
public record UserResponse
(
	Guid Id,
	string? Email,
	string? FullName,
	long? TelegramId,
	string? PhoneNumber,
	LanguageCodes LangCode,
	bool IsActive,
	string Source,
	IReadOnlyList<string> Roles,
	DateTime CreatedAt,
	DateTime UpdatedAt
);
