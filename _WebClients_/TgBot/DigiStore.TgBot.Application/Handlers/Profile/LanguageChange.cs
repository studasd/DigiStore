using CSharpFunctionalExtensions;
using DigiStore.Enums;
using DigiStore.TgBot.Application.Constants;
using DigiStore.TgBot.Application.Handlers.Adstracts;
using DigiStore.TgBot.Application.Interfaces.Services;
using Microsoft.Extensions.Logging;
using StudCoreKit.SharedKernel;
using StudCoreKit.SharedKernel.Extensions;
using StudTgBotApi.Contracts.Interfaces;
using Telegram.Bot.Types;

namespace DigiStore.TgBot.Application.Handlers.Profile;

/// <summary>
/// Обработчик колбэка смены языка из команды /language
/// </summary>
public class LanguageChange : BaseHandler, ICallbackHandler
{
	public const string CallbackData = Constants.CallbackData.LanguageChangePrefix;
	public const bool IsPrefix = true;

	private readonly IProfileService _profileService;
	private readonly ISessionService _sessionService;
	private readonly ILogger<LanguageChange> _logger;

	

	public LanguageChange(
		IBotAPIClient botClient,
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

	public async Task<UnitResult<Error>> HandleAsync(CallbackQuery callbackQuery, CancellationToken token = default)
	{
		// Handle language change from /language command

		if (callbackQuery.Data == null || callbackQuery.Message == null)
			return Error.Failure("callback.langchange.nodata", "No data in LanguageChangeCallbackHandler");

		
		var telegramId = callbackQuery.From.Id;
		var sessionResult = await _sessionService.GetSessionAsync(telegramId, token);

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

			//📍 Выберите язык:
			var text = _localService.GetMessage(LocalKeys.Messages.SelectLanguage, currentLanguage);

			var editResult = await _botClient.EditMessageTextAsync(
				callbackQuery.Message.Chat.Id,
				callbackQuery.Message.MessageId,
				text,
				replyMarkup: keyboard,
				cancellationToken: token);
			if (editResult.IsFailure)
			{
				_logger.LogError("Error editing message for language selection: {Error}", editResult.Error);
				return await AnswerCallbackQueryWithError(callbackQuery.Id, LanguageCodes.en, token);
			}

			return await _botClient.AnswerCallbackQueryAsync(callbackQuery.Id, cancellationToken: token);
		}

		// Change language

		_logger.LogInformation(
			"Language changed: {OldLanguage} -> {NewLanguage}, UserId: {UserId}",
			currentLanguage, languageCode, session.UserId);

		// Update user language
		var updateResult2 = await _profileService.UpdateUserLanguageAsync(
			session.UserId,
			languageCode,
			token);

		if (updateResult2.IsFailure)
		{
			_logger.LogError("Error updating user language: {Error}", updateResult2.Error);
			await AnswerCallbackQueryWithError(callbackQuery.Id, currentLanguage, token);
			return updateResult2.Error;
		}

		// Update session
		session.LangCode = languageCode;
		session.SetState(BotState.MainMenu);
		await _sessionService.UpdateSessionAsync(session, token);

		//✅ Язык изменён на Русский
		var confirmText = _localService.GetMessage(LocalKeys.Messages.LanguageChanged, languageCode);
		
		var keyboard2 = GetMainMenuKeyboard(languageCode);


		var editResult2 = await _botClient.EditMessageTextAsync(
			callbackQuery.Message.Chat.Id,
			callbackQuery.Message.MessageId,
			confirmText,
			replyMarkup: keyboard2,
			cancellationToken: token);

		if (editResult2.IsFailure)
		{
			_logger.LogError("Error editing message after language change: {Error}", editResult2.Error);
			return await AnswerCallbackQueryWithError(callbackQuery.Id, LanguageCodes.en, token);
		}

		
		_logger.LogInformation("Language updated successfully for user: {UserId}", session.UserId);

		return await _botClient.AnswerCallbackQueryAsync(callbackQuery.Id, cancellationToken: token);
	}
}
