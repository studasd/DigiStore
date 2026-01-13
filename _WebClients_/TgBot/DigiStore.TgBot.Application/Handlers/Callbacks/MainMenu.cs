using DigiStore.TgBot.Application.Constants;
using DigiStore.TgBot.Application.Handlers;
using DigiStore.TgBot.Application.Interfaces.Services;
using DigiStore.UserService.Contracts.Enums;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace DigiStore.TgBot.Application.Handlers.Callbacks;

/// <summary>
/// Обработчик колбэка главного меню
/// </summary>
public class MainMenu : BaseHandler, ICallbackQueryHandler
{
	public const string CallbackData = Constants.CallbackData.MenuMain;
	public const bool IsPrefix = false;
	
	private readonly ISessionService _sessionService;
	private readonly ILogger<MainMenu> _logger;


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
			var sessionResult = await _sessionService.GetSessionAsync(telegramId, cancellationToken);

			var languageCode = LanguageCodes.en;

			if(sessionResult.IsSuccess)
			{
				languageCode = sessionResult.Value.LangCode;
			}

			var text = $"{_localService.GetMessage(LocalKeys.Navigations.MainMenu, languageCode)}\n\n" +
					  $"{_localService.GetMessage(LocalKeys.Navigations.ChooseOption, languageCode)}";

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
			await AnswerCallbackQueryWithError(callbackQuery.Id, LanguageCodes.en, cancellationToken);
		}
	}
}
