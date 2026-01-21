using CSharpFunctionalExtensions;
using DigiStore.Enums;
using DigiStore.SharedKernel;
using DigiStore.TgBot.Application.Constants;
using DigiStore.TgBot.Application.Handlers.Adstracts;
using DigiStore.TgBot.Application.Interfaces;
using DigiStore.TgBot.Application.Interfaces.Services;
using Microsoft.Extensions.Logging;
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
		IBotAPIClient botClient,
		ISessionService sessionService,
		ILocalizationService localizationService,
		ILogger<MainMenu> logger)
		: base(botClient, localizationService)
	{
		_sessionService = sessionService;
		_logger = logger;
	}

	public async Task<UnitResult<Error>> HandleAsync(CallbackQuery callbackQuery, CancellationToken token = default)
	{
		// Handle main menu

		if (callbackQuery.Message == null)
			return Error.Failure("callback.mainmenu.nomessage", "No message in MainMenuCallbackHandler");


		var telegramId = callbackQuery.From.Id;
		var sessionResult = await _sessionService.GetSessionAsync(telegramId, token);

		var languageCode = LanguageCodes.en;

		if(sessionResult.IsSuccess)
			languageCode = sessionResult.Value.LangCode;


		var text = _localService.GetMessage(LocalKeys.Messages.MainMenu, languageCode);


		var keyboard = GetMainMenuKeyboard(languageCode);


		var editResult = await _botClient.EditMessageTextAsync(
			callbackQuery.Message.Chat.Id,
			callbackQuery.Message.MessageId,
			text,
			replyMarkup: keyboard,
			cancellationToken: token);

		if (editResult.IsFailure)
		{
			_logger.LogError("Error in MainMenuCallbackHandler");
			return await AnswerCallbackQueryWithError(callbackQuery.Id, LanguageCodes.en, token);
		}

		return await _botClient.AnswerCallbackQueryAsync(callbackQuery.Id, cancellationToken: token);
	}
}
