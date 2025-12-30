using DigiStore.TgBot.Application.Constants;
using DigiStore.TgBot.Application.Handlers.Attributes;
using DigiStore.TgBot.Application.Interfaces;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace DigiStore.TgBot.Application.Handlers.Callbacks;

/// <summary>
/// Обработчик колбэка главного меню
/// </summary>
[CallbackQuery(CallbackData.MenuMain)]
public class MainMenuCallbackHandler : ICallbackQueryHandler
{
	private readonly ITelegramSessionService _sessionService;
	private readonly ILocalizationService _localizationService;
	private readonly ILogger<MainMenuCallbackHandler> _logger;

	public MainMenuCallbackHandler(
		ITelegramSessionService sessionService,
		ILocalizationService localizationService,
		ILogger<MainMenuCallbackHandler> logger)
	{
		_sessionService = sessionService;
		_localizationService = localizationService;
		_logger = logger;
	}

	public async Task HandleAsync(ITelegramBotClient botClient, CallbackQuery callbackQuery, CancellationToken cancellationToken = default)
	{
		if (callbackQuery.Message == null)
			return;

		try
		{
			var telegramId = callbackQuery.From.Id;
			var session = await _sessionService.GetSessionAsync(telegramId, cancellationToken);

			var languageCode = session?.LanguageCode ?? "en";
			var loc = _localizationService;

			var text = $"{loc.GetMessage("main_menu", languageCode)}\n\n" +
					  $"{loc.GetMessage("choose_option", languageCode)}";

			var keyboard = GetMainMenuKeyboard(languageCode);

			await botClient.EditMessageText(
				callbackQuery.Message.Chat.Id,
				callbackQuery.Message.MessageId,
				text,
				replyMarkup: keyboard,
				cancellationToken: cancellationToken);

			await botClient.AnswerCallbackQuery(callbackQuery.Id, cancellationToken: cancellationToken);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error in MainMenuCallbackHandler");
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

