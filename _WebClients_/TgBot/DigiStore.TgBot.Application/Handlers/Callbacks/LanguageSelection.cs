using CSharpFunctionalExtensions;
using DigiStore.Enums;
using DigiStore.SharedKernel;
using DigiStore.SharedKernel.Extensions;
using DigiStore.TgBot.Application.Constants;
using DigiStore.TgBot.Application.Handlers.Adstracts;
using DigiStore.TgBot.Application.Interfaces.Services;
using DigiStore.TgBot.Domain;
using DigiStore.TgBot.Domain.ValueObjects;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace DigiStore.TgBot.Application.Handlers.Callbacks;

/// <summary>
/// Обработчик колбэка выбора языка из команды /start
/// </summary>
public class LanguageSelection : BaseHandler, ICallbackQueryHandler
{
	public const string CallbackData = Constants.CallbackData.LanguagePrefix;
	public const bool IsPrefix = true;

	private readonly IProfileService _profileService;
	private readonly ISessionService _sessionService;
	private readonly ILogger<LanguageSelection> _logger;

	

	public LanguageSelection(
		ITelegramBotClient botClient,
		IProfileService profileService,
		ISessionService sessionService,
		ILocalizationService localizationService,
		ILogger<LanguageSelection> logger)
		: base(botClient, localizationService)
	{
		_profileService = profileService;
		_sessionService = sessionService;
		_logger = logger;
	}

	public async Task<UnitResult<Error>> HandleAsync(CallbackQuery callbackQuery, CancellationToken token = default)
	{
		// Handle language selection from /start command

		if (callbackQuery.Data == null || callbackQuery.Message == null)
			return Error.Failure("callback.langselect.nodata", "No data in LanguageSelectionCallbackHandler");

		
		var telegramId = callbackQuery.From.Id;
		var sessionResult = await _sessionService.GetSessionAsync(telegramId, token);

		if (sessionResult.IsFailure)
			return sessionResult.Error;

		var session = sessionResult.Value;

		var langCodeResult = callbackQuery.Data.Replace(CallbackData, "").ParseEnum<LanguageCodes>();
		if(langCodeResult.IsFailure)
		{
			await AnswerCallbackQueryWithError(callbackQuery.Id, LanguageCodes.en, token);
			return langCodeResult.Error;
		}
		var langCode = langCodeResult.Value;

		_logger.LogInformation("Language selected from /start: {LanguageCode}, UserId: {UserId}", langCode, session.UserId);

		// Update user language in UserService
		var updateResult = await _profileService.UpdateUserLanguageAsync(
			session.UserId,
			langCode,
			token);

		if (updateResult.IsFailure)
		{
			await AnswerCallbackQueryWithError(callbackQuery.Id, langCode, token);
			return updateResult.Error;
		}

		// Update session
		session.LangCode = langCode;
		session.SetState(BotState.LanguageSelected);
		await _sessionService.UpdateSessionAsync(session, token);

		// Get full profile
		var profileResult = await _profileService.GetFullProfileAsync(
			session.UserId,
			session.TelegramId,
			token);

		if (profileResult.IsFailure)
		{
			await AnswerCallbackQueryWithError(callbackQuery.Id, langCode, token);
			return profileResult.Error;
		}

		var profile = profileResult.Value!;

		// Cache profile in session
		session.CachedProfile = new CachedUserProfileVO
		{
			UserId = profile.UserId,
			TelegramId = profile.TelegramId,
			Email = profile.Email,
			FirstName = profile.FullName.Split(' ').FirstOrDefault() ?? string.Empty,
			LastName = profile.FullName.Split(' ').LastOrDefault() ?? string.Empty,
			Username = profile.Username,
			LangCode = profile.LangCode,
			IsActive = profile.IsActive,
			Roles = profile.Roles,
			Balance = profile.Balance,
			Currency = profile.Currency,
			CreatedAt = profile.CreatedAt,
			UpdatedAt = profile.UpdatedAt
		};

		session.SetState(BotState.ProfileViewing);
		await _sessionService.UpdateSessionAsync(session, token);

		// Format and send profile
		var profileText = _profileService.FormatProfileText(profile, langCode);
		var keyboard = GetProfileKeyboard(langCode);


		try
		{
			await _botClient.EditMessageText(
				callbackQuery.Message.Chat.Id,
				callbackQuery.Message.MessageId,
				profileText,
				parseMode: ParseMode.Html,
				replyMarkup: keyboard,
				cancellationToken: token);

			await _botClient.AnswerCallbackQuery(callbackQuery.Id, cancellationToken: token);

			_logger.LogInformation(
				"Profile shown after language selection for user: {UserId}",
				session.UserId);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error in LanguageSelectionCallbackHandler");
			await AnswerCallbackQueryWithError(callbackQuery.Id, LanguageCodes.en, token);
			return Error.Failure("callback.langselect.error", "Error in LanguageSelectionCallbackHandler");
		}

		return Result.Success<Error>();
	}
}
