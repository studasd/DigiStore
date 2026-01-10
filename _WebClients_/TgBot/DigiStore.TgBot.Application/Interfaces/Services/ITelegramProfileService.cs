using CSharpFunctionalExtensions;
using DigiStore.SharedKernel;
using DigiStore.TgBot.Application.DTOs;
using Telegram.Bot.Types.ReplyMarkups;

namespace DigiStore.TgBot.Application.Interfaces.Services;

/// <summary>
/// Service для управления профилем пользователя в телеграме
/// </summary>
public interface ITelegramProfileService
{
	/// <summary>
	/// Get full profile with balance (для /profile и /start)
	/// </summary>
	Task<Result<ProfileDisplayDto, Error>> GetFullProfileAsync(Guid userId, long telegramId, CancellationToken ct = default);

	/// <summary>
	/// Format profile to readable text for Telegram
	/// </summary>
	string FormatProfileText(ProfileDisplayDto profile, string languageCode);

	/// <summary>
	/// Update user language and session
	/// </summary>
	Task<Result<bool, Error>> UpdateUserLanguageAsync(Guid userId, string languageCode, CancellationToken ct = default);

	/// <summary>
	/// Build profile message with keyboard
	/// </summary>
	(string text, InlineKeyboardMarkup keyboard) BuildProfileMessage(ProfileDisplayDto profile, string languageCode);
}