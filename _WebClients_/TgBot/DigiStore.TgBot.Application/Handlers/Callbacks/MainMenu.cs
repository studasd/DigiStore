using DigiStore.TgBot.Application.Constants;
using DigiStore.TgBot.Application.Handlers;
using DigiStore.TgBot.Application.Interfaces.Services;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace DigiStore.TgBot.Application.Handlers.Callbacks;

/// <summary>
/// Обработчик колбэка главного меню
/// </summary>
public class MainMenu : BaseHandler, ICallbackQueryHandler
{
	public const string CallbackData = DigiStore.TgBot.Application.Constants.CallbackData.MenuMain;
	public const bool IsPrefix = false;
	
	private readonly ISessionService _sessionService;
	private readonly ILogger<MainMenu> _logger;

	string ICallbackQueryHandler.CallbackData => CallbackData;
	bool ICallbackQueryHandler.IsPrefix => IsPrefix;

	public MainMenu(
		ITelegramBotClient botClient,
		ISessionService sessionService,
		ILocalizationService localizationService,
		ILogger<MainMenu> logger)
		: base(botClient, localizationService)
	{
		_sessionService = sessionService;
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

			var languageCode = session?.LangCode ?? "en";

			var text = $"{_localizationService.GetMessage("main_menu", languageCode)}\n\n" +
					  $"{_localizationService.GetMessage("choose_option", languageCode)}";

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
			await AnswerCallbackQueryWithError(callbackQuery.Id, "en", cancellationToken);
		}
	}
}
