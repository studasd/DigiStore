using CSharpFunctionalExtensions;
using DigiStore.Enums;
using StudCoreKit.SharedKernel;
using DigiStore.TgBot.Application.Constants;
using DigiStore.TgBot.Application.DTOs;
using DigiStore.TgBot.Application.Interfaces.Services;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Types.ReplyMarkups;

namespace DigiStore.TgBot.Application.Services;

public class ProfileService : IProfileService
{
	private readonly ITgUserService _userService;
	private readonly IWalletService _walletService;
	private readonly ILocalizationService _localizationService;
	private readonly ILogger<ProfileService> _logger;
	public ProfileService(
		ITgUserService userService,
		IWalletService walletService,
		ILocalizationService localizationService,
		ILogger<ProfileService> logger)
	{
		_userService = userService;
		_walletService = walletService;
		_localizationService = localizationService;
		_logger = logger;
	}

	public async Task<Result<ProfileDisplayDto, Error>> GetFullProfileAsync(Guid userId, long telegramId, CancellationToken token)
	{
		// Get user profile
		var userResult = await _userService.GetUserProfileAsync(userId, token);
		if (!userResult.IsSuccess)
		{
			_logger.LogWarning("Failed to get user profile for user ID: {UserId}", userId);
			return userResult.Error;
		}
		var user = userResult.Value!;
			
		// Get wallet/balance
		var walletResult = await _walletService.GetBalanceAsync(userId, token);
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


	public string FormatProfileText(ProfileDisplayDto profile, LanguageCodes langCode)
	{
		var loc = _localizationService;
		var lang = langCode;


		//════════════════════════════════════
		//  ВАШ ПРОФИЛЬ 
		//════════════════════════════════════
		//👤 Имя: Stavs
		//🆔 Telegram ID: 307723779
		//📧 Email: telegram_307723779 @digistore.local

		//💰 Баланс: 1980.00 RUB
		//🌐 Язык: 🇷🇺 Русский
		//✅ Статус: Active
		//🎖️ Роли: User

		//📅 Дата регистрации: 21.01.2026 12:28
		//🔄 Последнее обновление: 21.01.2026 12:28


		//════════════════════════════════════
		//  ВАШ ПРОФИЛЬ 
		//════════════════════════════════════
		//👤 Имя: {{fullname}}
		//🆔 Telegram ID: {{telegramid}}
		//📧 Email: {{email}}
		//📱  Username: {{username}}

		//💰 Баланс: {{balance}} {{currency}}
		//🌐 Язык: {{language}}
		//✅ Статус: {{status}}
		//🎖️ Роли: {{roles}}

		//📅 Дата регистрации: {{datecreated}}
		//🔄 Последнее обновление: {{dateupdated}}


		var model = new
		{
			fullname = profile.FullName,
			telegramid = profile.TelegramId,
			email = profile.Email,
			username = profile.Username,
			balance = profile.Balance,
			currency = profile.Currency,
			language = GetLanguageName(profile.LangCode),
			status = profile.IsActive ? "Active" : "Inactive",
			roles = profile.Roles.Count > 0 ? string.Join(", ", profile.Roles) : "None",
			datecreated = profile.CreatedAt.ToString("dd.MM.yyyy HH:mm"),
			dateupdated = profile.UpdatedAt.ToString("dd.MM.yyyy HH:mm")
		};

		var text = loc.GetMessage(LocalKeys.Templates.Profile, lang, model);


		return text;
	}


	public async Task<UnitResult<Error>> UpdateUserLanguageAsync(
		Guid userId,
		LanguageCodes langCode,
		CancellationToken token)
	{
		var result = await _userService.UpdateLanguageAsync(userId, langCode, token);
		if (result.IsFailure)
		{
			_logger.LogWarning("Failed to update language for user ID: {UserId}", userId);
			return result.Error;
		}

		_logger.LogInformation("Language updated for user ID: {UserId} to {LanguageCode}", userId, langCode);
		return result;
	}


	public (string text, InlineKeyboardMarkup keyboard) BuildProfileMessage(ProfileDisplayDto profile, LanguageCodes langCode)
	{
		var text = FormatProfileText(profile, langCode);

		var keyboard = new InlineKeyboardMarkup(new[]
		{
			new[]
			{
				InlineKeyboardButton.WithCallbackData(
					_localizationService.GetMessage(LocalKeys.Buttons.ChangeLanguage, langCode),
					CallbackData.MenuBack)
			},
			new[]
			{
				InlineKeyboardButton.WithCallbackData(
					_localizationService.GetMessage(LocalKeys.Buttons.MainMenu, langCode),
					CallbackData.MenuMain)
			},
		});

		return (text, keyboard);
	}


	private string GetLanguageName(LanguageCodes langCode)
	{
		return langCode switch
		{
			LanguageCodes.en => "🇬🇧 English",
			LanguageCodes.ru => "🇷🇺 Русский",
			_ => langCode.ToString()
		};
	}
}
