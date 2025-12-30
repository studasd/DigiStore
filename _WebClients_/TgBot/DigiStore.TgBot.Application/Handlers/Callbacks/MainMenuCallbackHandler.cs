using DigiStore.TgBot.Application.Constants;
using DigiStore.TgBot.Application.Interfaces;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace DigiStore.TgBot.Application.Handlers.Callbacks;

/// <summary>
/// Обработчик колбэка главного меню
/// </summary>
public class MainMenuCallbackHandler : ICallbackQueryHandler
{
	public const string CallbackData = DigiStore.TgBot.Application.Constants.CallbackData.MenuMain;
	public const bool IsPrefix = false;
	
	private readonly ITelegramBotClient _botClient;
	private readonly ITelegramSessionService _sessionService;
	private readonly ILocalizationService _localizationService;
	private readonly ILogger<MainMenuCallbackHandler> _logger;

	string ICallbackQueryHandler.CallbackData => CallbackData;
	bool ICallbackQueryHandler.IsPrefix => IsPrefix;

	public MainMenuCallbackHandler(
		ITelegramBotClient botClient,
		ITelegramSessionService sessionService,
		ILocalizationService localizationService,
		ILogger<MainMenuCallbackHandler> logger)
	{
		_botClient = botClient;
		_sessionService = sessionService;
		_localizationService = localizationService;
		_logger = logger;
	}

	public async Task HandleAsync(CallbackQuery callbackQuery, CancellationToken cancellationToken = default)
	{
		// Handle main menu

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

			await _botClient.EditMessageText(
				callbackQuery.Message.Chat.Id,
				callbackQuery.Message.MessageId,
				text,
				replyMarkup: keyboard,
				cancellationToken: cancellationToken);

			await _botClient.AnswerCallbackQuery(callbackQuery.Id, cancellationToken: cancellationToken);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error in MainMenuCallbackHandler");
			try
			{
				await _botClient.AnswerCallbackQuery(
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
					DigiStore.TgBot.Application.Constants.CallbackData.ProfileView)
			},
			new[]
			{
				InlineKeyboardButton.WithCallbackData(
					loc.GetMessage("balance", languageCode),
					DigiStore.TgBot.Application.Constants.CallbackData.BalanceView)
			},
			new[]
			{
				InlineKeyboardButton.WithCallbackData(
					loc.GetMessage("catalog", languageCode),
					DigiStore.TgBot.Application.Constants.CallbackData.CatalogView)
			},
		});
	}
}
