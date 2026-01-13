using CSharpFunctionalExtensions;
using DigiStore.SharedKernel;
using DigiStore.SharedKernel.Extensions;
using DigiStore.TgBot.Application.Constants;
using DigiStore.TgBot.Application.Handlers.Adstracts;
using DigiStore.TgBot.Application.Interfaces.Services;
using DigiStore.UserService.Contracts.Enums;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace DigiStore.TgBot.Application.Handlers.Callbacks;

/// <summary>
/// Обработчик колбэка смены языка из команды /language
/// </summary>
public class LanguageChange : BaseHandler, ICallbackQueryHandler
{
	public const string CallbackData = Constants.CallbackData.LanguageChangePrefix;
	public const bool IsPrefix = true;

	private readonly IProfileService _profileService;
	private readonly ISessionService _sessionService;
	private readonly ILogger<LanguageChange> _logger;

	

	public LanguageChange(
		ITelegramBotClient botClient,
		IProfileService profileService,
		ISessionService sessionService,
		ILocalizationService localizationService,
		ILogger<LanguageChange> logger)
		: base(botClient, localizationService)
	{
		_profileService = profileService;
		_sessionService = sessionService;
		_logger = logger;
	}

	public async Task<UnitResult<Error>> HandleAsync(CallbackQuery callbackQuery, CancellationToken cancellationToken = default)
	{
		// Handle language change from /language command

		if (callbackQuery.Data == null || callbackQuery.Message == null)
			return Error.Failure("callback.langchange.nodata", "No data in LanguageChangeCallbackHandler");

		
		var telegramId = callbackQuery.From.Id;
		var sessionResult = await _sessionService.GetSessionAsync(telegramId, cancellationToken);

		if (sessionResult.IsFailure)
		{
			_logger.LogWarning("Session not found for TelegramId: {TelegramId}", telegramId);
			return sessionResult.Error;
		}

		var session = sessionResult.Value;

		var data = callbackQuery.Data;
		var languageResult = data.Replace(CallbackData, "").ParseEnum<LanguageCodes>();

		if (languageResult.IsFailure)
		{
			return languageResult.Error;
		}

		var languageCode = languageResult.Value;
		var currentLanguage = session.LangCode;

		// Handle "select" case - shows all languages
		if (languageCode == LanguageCodes.select)
		{
			var keyboard = GetLanguageSelectionKeyboard(CallbackData);
			var text = _localService.GetMessage(LocalKeys.Navigations.SelectLanguage, currentLanguage);

			await _botClient.EditMessageText(
				callbackQuery.Message.Chat.Id,
				callbackQuery.Message.MessageId,
				text,
				replyMarkup: keyboard,
				cancellationToken: cancellationToken);

			await _botClient.AnswerCallbackQuery(callbackQuery.Id, cancellationToken: cancellationToken);
			return Result.Success<Error>();
		}

		// Change language

		_logger.LogInformation(
			"Language changed: {OldLanguage} -> {NewLanguage}, UserId: {UserId}",
			currentLanguage, languageCode, session.UserId);

		// Update user language
		var updateResult = await _profileService.UpdateUserLanguageAsync(
			session.UserId,
			languageCode,
			cancellationToken);

		if (updateResult.IsFailure)
		{
			await AnswerCallbackQueryWithError(callbackQuery.Id, currentLanguage, cancellationToken);
			return updateResult.Error;
		}

		// Update session
		session.LangCode = languageCode;
		session.SetState(BotState.MainMenu);
		await _sessionService.UpdateSessionAsync(session, cancellationToken);

		var confirmText = _localService.GetMessage(LocalKeys.Navigations.LanguageChanged, languageCode);
		var keyboard2 = GetMainMenuKeyboard(languageCode);

		try
		{
			await _botClient.EditMessageText(
				callbackQuery.Message.Chat.Id,
				callbackQuery.Message.MessageId,
				confirmText,
				replyMarkup: keyboard2,
				cancellationToken: cancellationToken);

			await _botClient.AnswerCallbackQuery(callbackQuery.Id, cancellationToken: cancellationToken);

			_logger.LogInformation(
				"Language updated successfully for user: {UserId}",
				session.UserId);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error in LanguageChangeCallbackHandler");
			await AnswerCallbackQueryWithError(callbackQuery.Id, LanguageCodes.en, cancellationToken);
			return Error.Failure("callback.langchange.error", "Error in LanguageChangeCallbackHandler");
		}

		return Result.Success<Error>();
	}
}
