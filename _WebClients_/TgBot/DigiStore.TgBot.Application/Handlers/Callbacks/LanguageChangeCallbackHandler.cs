using DigiStore.TgBot.Application.Constants;
using DigiStore.TgBot.Application.Handlers.Attributes;
using DigiStore.TgBot.Application.Interfaces;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace DigiStore.TgBot.Application.Handlers.Callbacks;

/// <summary>
/// Обработчик колбэка смены языка из команды /language
/// </summary>
[CallbackQuery(CallbackData.LanguageChangePrefix, IsPrefix = true)]
public class LanguageChangeCallbackHandler : ICallbackQueryHandler
{
	private readonly ITelegramProfileService _profileService;
	private readonly ITelegramSessionService _sessionService;
	private readonly ILocalizationService _localizationService;
	private readonly ILogger<LanguageChangeCallbackHandler> _logger;

	public LanguageChangeCallbackHandler(
		ITelegramProfileService profileService,
		ITelegramSessionService sessionService,
		ILocalizationService localizationService,
		ILogger<LanguageChangeCallbackHandler> logger)
	{
		_profileService = profileService;
		_sessionService = sessionService;
		_localizationService = localizationService;
		_logger = logger;
	}

	public async Task HandleAsync(ITelegramBotClient botClient, CallbackQuery callbackQuery, CancellationToken cancellationToken = default)
	{
		if (callbackQuery.Data == null || callbackQuery.Message == null)
			return;

		try
		{
			var telegramId = callbackQuery.From.Id;
			var session = await _sessionService.GetSessionAsync(telegramId, cancellationToken)
				?? throw new InvalidOperationException("Session not found");

			var data = callbackQuery.Data;
			var languageCode = data.Replace(CallbackData.LanguageChangePrefix, "");
			var currentLanguage = session.LanguageCode ?? "en";

			// Handle "select" case - shows all languages
			if (languageCode == "select")
			{
				var languages = _localizationService.GetLanguages();
				var buttons = new List<List<InlineKeyboardButton>>();

				foreach (var lang in languages)
				{
					buttons.Add(new List<InlineKeyboardButton>
					{
						InlineKeyboardButton.WithCallbackData(
							lang.Value,
							$"{CallbackData.LanguageChangePrefix}{lang.Key}")
					});
				}

				var keyboard = new InlineKeyboardMarkup(buttons);
				var text = _localizationService.GetMessage("select_language", currentLanguage);

				await botClient.EditMessageText(
					callbackQuery.Message.Chat.Id,
					callbackQuery.Message.MessageId,
					text,
					replyMarkup: keyboard,
					cancellationToken: cancellationToken);

				await botClient.AnswerCallbackQuery(callbackQuery.Id, cancellationToken: cancellationToken);
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
				await botClient.AnswerCallbackQuery(
					callbackQuery.Id,
					_localizationService.GetMessage("error_occurred", currentLanguage),
					showAlert: true,
					cancellationToken: cancellationToken);
				return;
			}

			// Update session
			session.LanguageCode = languageCode;
			session.SetState(BotState.MainMenu);
			await _sessionService.UpdateSessionAsync(session, cancellationToken);

			var confirmText = _localizationService.GetMessage("language_changed", languageCode);
			var keyboard = GetMainMenuKeyboard(languageCode);

			await botClient.EditMessageText(
				callbackQuery.Message.Chat.Id,
				callbackQuery.Message.MessageId,
				confirmText,
				replyMarkup: keyboard,
				cancellationToken: cancellationToken);

			await botClient.AnswerCallbackQuery(callbackQuery.Id, cancellationToken: cancellationToken);

			_logger.LogInformation(
				"Language updated successfully for user: {UserId}",
				session.UserId);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error in LanguageChangeCallbackHandler");
			try
			{
				await botClient.AnswerCallbackQuery(
					callbackQuery.Id,
					_localizationService.GetMessage("error_occurred", "en"),
					showAlert: true,
					cancellationToken: cancellationToken);
			}
			catch { }
		}
	}

	private InlineKeyboardMarkup GetMainMenuKeyboard(string languageCode)
	{
		var loc = _localizationService;

		return new InlineKeyboardMarkup(new[]
		{
			new[]
			{
				InlineKeyboardButton.WithCallbackData(
					loc.GetMessage("profile", languageCode),
					CallbackData.ProfileView)
			},
			new[]
			{
				InlineKeyboardButton.WithCallbackData(
					loc.GetMessage("balance", languageCode),
					CallbackData.BalanceView)
			},
			new[]
			{
				InlineKeyboardButton.WithCallbackData(
					loc.GetMessage("catalog", languageCode),
					CallbackData.CatalogView)
			},
		});
	}
}

