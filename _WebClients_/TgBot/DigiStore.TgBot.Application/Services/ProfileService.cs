using CSharpFunctionalExtensions;
using DigiStore.SharedKernel;
using DigiStore.TgBot.Application.Constants;
using DigiStore.TgBot.Application.DTOs;
using DigiStore.TgBot.Application.Interfaces.Services;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Types.ReplyMarkups;

namespace DigiStore.TgBot.Application.Services;

public class ProfileService : IProfileService
{
	private readonly IUserService _userService;
	private readonly IWalletService _walletService;
	private readonly ILocalizationService _localizationService;
	private readonly ILogger<ProfileService> _logger;
	public ProfileService(
		IUserService userService,
		IWalletService walletService,
		ILocalizationService localizationService,
		ILogger<ProfileService> logger)
	{
		_userService = userService;
		_walletService = walletService;
		_localizationService = localizationService;
		_logger = logger;
	}

	public async Task<Result<ProfileDisplayDto, Error>> GetFullProfileAsync(Guid userId, long telegramId, CancellationToken ct = default)
	{
		try
		{
			// Get user profile
			var userResult = await _userService.GetUserProfileAsync(userId, ct);
			if (!userResult.IsSuccess)
			{
				_logger.LogWarning("Failed to get user profile for user ID: {UserId}", userId);
				return userResult.Error;
			}
			var user = userResult.Value!;
			
			// Get wallet/balance
			var walletResult = await _walletService.GetBalanceAsync(userId, ct);
			decimal balance = 0;
			string currency = "RUB";
			if (walletResult.IsSuccess)
			{
				balance = walletResult.Value!.Balance;
				currency = walletResult.Value.Currency;
			}
			
			var profile = new ProfileDisplayDto
			{
				TelegramId = telegramId,
				UserId = userId,
				FullName = user.FullName,
				Email = user.Email,
				Username = user.Username,
				Balance = balance,
				Currency = currency,
				LangCode = user.LangCode,
				IsActive = user.IsActive,
				Roles = user.Roles,
				CreatedAt = DateTime.UtcNow,
				UpdatedAt = DateTime.UtcNow
			};
			_logger.LogInformation("Full profile retrieved for user ID: {UserId}", userId);
			
			return profile;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error getting full profile for user ID: {UserId}", userId);
			return Error.Failure("profile.retrieval_error", ex.Message);
		}
	}


	public string FormatProfileText(ProfileDisplayDto profile, string languageCode)
	{
		var loc = _localizationService;
		var lang = languageCode.ToLower();
		var text = $@"
╔════════════════════════════════════╗
║ {loc.GetMessage("profile_info", lang)}
╚════════════════════════════════════╝
👤 {loc.GetMessage("full_name", lang)}: {profile.FullName}
🆔 Telegram ID: {profile.TelegramId}
📧 {loc.GetMessage("email", lang)}: {profile.Email}
";
		if (!string.IsNullOrEmpty(profile.Username))
		{
			text += $"📱 {loc.GetMessage("telegram_username", lang)}: @{profile.Username}\n";
		}

		text += $@"
💰 {loc.GetMessage("balance", lang)}: {profile.Balance:F2} {profile.Currency}
🌐 {loc.GetMessage("language", lang)}: {GetLanguageName(profile.LangCode)}
✅ {loc.GetMessage("status", lang)}: {(profile.IsActive ? "Active" : "Inactive")}
";
		if (profile.Roles.Count > 0)
		{
			text += $"🎖️ {loc.GetMessage("roles", lang)}: {string.Join(", ", profile.Roles)}\n";
		}

		text += $@"
📅 {loc.GetMessage("created_at", lang)}: {profile.CreatedAt:dd.MM.yyyy HH:mm}
🔄 {loc.GetMessage("updated_at", lang)}: {profile.UpdatedAt:dd.MM.yyyy HH:mm}
";
		return text;
	}


	public async Task<Result<bool, Error>> UpdateUserLanguageAsync(
		Guid userId,
		string languageCode,
		CancellationToken ct = default)
	{
		try
		{
			var result = await _userService.UpdateLanguageAsync(userId, languageCode, ct);
			if (!result.IsSuccess)
			{
				_logger.LogWarning("Failed to update language for user ID: {UserId}", userId);
				return result;
			}

			_logger.LogInformation("Language updated for user ID: {UserId} to {LanguageCode}", userId, languageCode);
			return true;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error updating language for user ID: {UserId}", userId);
			return Error.Failure("profile.language_update_error", ex.Message);
		}
	}


	public (string text, InlineKeyboardMarkup keyboard) BuildProfileMessage(ProfileDisplayDto profile, string languageCode)
	{
		var text = FormatProfileText(profile, languageCode);

		var keyboard = new InlineKeyboardMarkup(new[]
		{
			new[]
			{
				InlineKeyboardButton.WithCallbackData(
					_localizationService.GetMessage("change_language", languageCode),
					CallbackData.MenuBack)
			},
			new[]
			{
				InlineKeyboardButton.WithCallbackData(
					_localizationService.GetMessage("main_menu", languageCode),
					CallbackData.MenuMain)
			},
		});

		return (text, keyboard);
	}


	private string GetLanguageName(string languageCode)
	{
		return languageCode.ToLower() switch
		{
			"en" => "🇬🇧 English",
			"ru" => "🇷🇺 Русский",
			"es" => "🇪🇸 Español",
			"de" => "🇩🇪 Deutsch",
			"fr" => "🇫🇷 Français",
			_ => languageCode
		};
	}
}
