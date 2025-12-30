using CSharpFunctionalExtensions;
using DigiStore.SharedKernel;
using DigiStore.TgBot.Application.DTOs;

namespace DigiStore.TgBot.Application.Interfaces;


/// <summary>
/// Service to interact with UserService via HTTP
/// </summary>
public interface ITelegramUserService
{
	/// <summary>
	/// Get or create user by Telegram ID
	/// </summary>
	Task<Result<TelegramUserDto, Error>> GetOrCreateUserAsync(
		long telegramId,
		string? username,
		string? firstName,
		string? lastName,
		string languageCode,
		CancellationToken ct = default);

	/// <summary>
	/// Get user profile
	/// </summary>
	Task<Result<TelegramUserDto, Error>> GetUserProfileAsync(Guid userId, CancellationToken ct = default);

	/// <summary>
	/// Update language preference
	/// </summary>
	Task<Result<bool, Error>> UpdateLanguageAsync(Guid userId, string languageCode, CancellationToken ct = default);

	/// <summary>
	/// Update user activity
	/// </summary>
	Task<Result<bool, Error>> UpdateActivityAsync(Guid userId, CancellationToken ct = default);
}
