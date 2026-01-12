using DigiStore.SharedKernel.Extensions;
using DigiStore.TgBot.Application.Constants;
using DigiStore.TgBot.Application.Handlers;
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
	public const string CallbackData = DigiStore.TgBot.Application.Constants.CallbackData.LanguageChangePrefix;
	public const bool IsPrefix = true;
	
	private readonly IProfileService _profileService;
	private readonly ISessionService _sessionService;
	private readonly ILogger<LanguageChange> _logger;

	string ICallbackQueryHandler.CallbackData => CallbackData;
	bool ICallbackQueryHandler.IsPrefix => IsPrefix;

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

	public async Task HandleAsync(CallbackQuery callbackQuery, CancellationToken cancellationToken = default)
	{
		// Handle language change from /language command

		if (callbackQuery.Data == null || callbackQuery.Message == null)
			return;

		try
		{
			var telegramId = callbackQuery.From.Id;
			var session = await _sessionService.GetSessionAsync(telegramId, cancellationToken)
				?? throw new InvalidOperationException("Session not found");

			var data = callbackQuery.Data;
			var languageCode = data.Replace(CallbackData, "").ParseEnum<LanguageCodes>().Value;
			var currentLanguage = session.LangCode;

			// Handle "select" case - shows all languages
			if (languageCode == LanguageCodes.select)
			{
				var keyboard = GetLanguageSelectionKeyboard(CallbackData);
				var text = _localizationService.GetMessage(LocalKeys.Navigations.SelectLanguage, currentLanguage);

				await _botClient.EditMessageText(
					callbackQuery.Message.Chat.Id,
					callbackQuery.Message.MessageId,
					text,
					replyMarkup: keyboard,
					cancellationToken: cancellationToken);

				await _botClient.AnswerCallbackQuery(callbackQuery.Id, cancellationToken: cancellationToken);
				return;
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

			if (!updateResult.IsSuccess)
			{
				await AnswerCallbackQueryWithError(callbackQuery.Id, currentLanguage, cancellationToken);
				return;
			}

			// Update session
			session.LangCode = languageCode;
			session.SetState(BotState.MainMenu);
			await _sessionService.UpdateSessionAsync(session, cancellationToken);

			var confirmText = _localizationService.GetMessage(LocalKeys.Navigations.LanguageChanged, languageCode);
			var keyboard2 = GetMainMenuKeyboard(languageCode);

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
		}
	}
}
