using DigiStore.TgBot.Application.Constants;
using DigiStore.TgBot.Application.Handlers;
using DigiStore.TgBot.Application.Interfaces;
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
	
	private readonly ITelegramProfileService _profileService;
	private readonly ITelegramSessionService _sessionService;
	private readonly ILogger<LanguageChange> _logger;

	string ICallbackQueryHandler.CallbackData => CallbackData;
	bool ICallbackQueryHandler.IsPrefix => IsPrefix;

	public LanguageChange(
		ITelegramBotClient botClient,
		ITelegramProfileService profileService,
		ITelegramSessionService sessionService,
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
			var languageCode = data.Replace(CallbackData, "");
			var currentLanguage = session.LanguageCode ?? "en";

			// Handle "select" case - shows all languages
			if (languageCode == "select")
			{
				var keyboard = GetLanguageSelectionKeyboard(CallbackData);
				var text = _localizationService.GetMessage("select_language", currentLanguage);

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
			if (!session.UserId.HasValue)
				return;

			_logger.LogInformation(
				"Language changed: {OldLanguage} -> {NewLanguage}, UserId: {UserId}",
				currentLanguage, languageCode, session.UserId);

			// Update user language
			var updateResult = await _profileService.UpdateUserLanguageAsync(
				session.UserId.Value,
				languageCode,
				cancellationToken);

			if (!updateResult.IsSuccess)
			{
				await AnswerCallbackQueryWithError(callbackQuery.Id, currentLanguage, cancellationToken);
				return;
			}

			// Update session
			session.LanguageCode = languageCode;
			session.SetState(BotState.MainMenu);
			await _sessionService.UpdateSessionAsync(session, cancellationToken);

			var confirmText = _localizationService.GetMessage("language_changed", languageCode);
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
			await AnswerCallbackQueryWithError(callbackQuery.Id, "en", cancellationToken);
		}
	}
}
