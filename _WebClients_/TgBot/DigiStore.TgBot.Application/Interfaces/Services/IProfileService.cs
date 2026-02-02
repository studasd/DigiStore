using CSharpFunctionalExtensions;
using DigiStore.Enums;
using StudCoreKit.SharedKernel;
using DigiStore.TgBot.Application.DTOs;
using Telegram.Bot.Types.ReplyMarkups;

namespace DigiStore.TgBot.Application.Interfaces.Services;

/// <summary>
/// Service для управления профилем пользователя в телеграме
/// </summary>
public interface IProfileService
{
	/// <summary>
	/// Get full profile with balance (для /profile и /start)
	/// </summary>
	Task<Result<ProfileDisplayDto, Error>> GetFullProfileAsync(Guid userId, long telegramId, CancellationToken token);

	/// <summary>
	/// Format profile to readable text for Telegram
	/// </summary>
	string FormatProfileText(ProfileDisplayDto profile, LanguageCodes langCode);

	/// <summary>
	/// Update user language and session
	/// </summary>
	Task<UnitResult<Error>> UpdateUserLanguageAsync(Guid userId, LanguageCodes langCode, CancellationToken token);

	/// <summary>
	/// Build profile message with keyboard
	/// </summary>
	(string text, InlineKeyboardMarkup keyboard) BuildProfileMessage(ProfileDisplayDto profile, LanguageCodes langCode);
}