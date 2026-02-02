using CSharpFunctionalExtensions;
using DigiStore.Enums;
using StudCoreKit.SharedKernel;
using DigiStore.TgBot.Application.DTOs;

namespace DigiStore.TgBot.Application.Interfaces.Services;


/// <summary>
/// Service to interact with UserService via HTTP
/// </summary>
public interface ITgUserService
{
	/// <summary>
	/// Get or create user by Telegram ID
	/// </summary>
	Task<Result<TgUserDto, Error>> GetOrCreateUserAsync(
		long telegramId,
		string? username,
		string? firstName,
		string? lastName,
		LanguageCodes langCode,
		CancellationToken token);

	/// <summary>
	/// Get user profile
	/// </summary>
	Task<Result<TgUserDto, Error>> GetUserProfileAsync(Guid userId, CancellationToken token);

	/// <summary>
	/// Update language preference
	/// </summary>
	Task<UnitResult<Error>> UpdateLanguageAsync(Guid userId, LanguageCodes langCode, CancellationToken token);

	/// <summary>
	/// Update user activity
	/// </summary>
	Task<UnitResult<Error>> UpdateActivityAsync(Guid userId, CancellationToken token);
}
