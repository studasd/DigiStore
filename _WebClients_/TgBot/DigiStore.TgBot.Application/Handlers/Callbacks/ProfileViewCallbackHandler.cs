using DigiStore.TgBot.Application.Constants;
using DigiStore.TgBot.Application.Interfaces;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace DigiStore.TgBot.Application.Handlers.Callbacks;

/// <summary>
/// Обработчик колбэка просмотра профиля
/// </summary>
public class ProfileViewCallbackHandler : ICallbackQueryHandler
{
	public const string CallbackData = DigiStore.TgBot.Application.Constants.CallbackData.ProfileView;
	public const bool IsPrefix = false;
	
	private readonly ITelegramBotClient _botClient;
	private readonly ITelegramProfileService _profileService;
	private readonly ITelegramSessionService _sessionService;
	private readonly ILocalizationService _localizationService;
	private readonly ILogger<ProfileViewCallbackHandler> _logger;

	string ICallbackQueryHandler.CallbackData => CallbackData;
	bool ICallbackQueryHandler.IsPrefix => IsPrefix;

	public ProfileViewCallbackHandler(
		ITelegramBotClient botClient,
		ITelegramProfileService profileService,
		ITelegramSessionService sessionService,
		ILocalizationService localizationService,
		ILogger<ProfileViewCallbackHandler> logger)
	{
		_botClient = botClient;
		_profileService = profileService;
		_sessionService = sessionService;
		_localizationService = localizationService;
		_logger = logger;
	}

	public async Task HandleAsync(CallbackQuery callbackQuery, CancellationToken cancellationToken = default)
	{
		// Handle profile view callback

		if (callbackQuery.Message == null)
			return;

		try
		{
			var telegramId = callbackQuery.From.Id;
			var session = await _sessionService.GetSessionAsync(telegramId, cancellationToken);

			if (session?.UserId == null)
				return;

			var languageCode = session.LanguageCode ?? "en";

			var profileResult = await _profileService.GetFullProfileAsync(
				session.UserId.Value,
				session.TelegramId,
				cancellationToken);

			if (!profileResult.IsSuccess)
			{
				await _botClient.AnswerCallbackQuery(
					callbackQuery.Id,
					_localizationService.GetMessage("error_occurred", languageCode),
					showAlert: true,
					cancellationToken: cancellationToken);
				return;
			}

			var profile = profileResult.Value!;
			var profileText = _profileService.FormatProfileText(profile, languageCode);
			var keyboard = GetProfileKeyboard(languageCode);

			await _botClient.EditMessageText(
				callbackQuery.Message.Chat.Id,
				callbackQuery.Message.MessageId,
				profileText,
				parseMode: ParseMode.Html,
				replyMarkup: keyboard,
				cancellationToken: cancellationToken);

			await _botClient.AnswerCallbackQuery(callbackQuery.Id, cancellationToken: cancellationToken);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error in ProfileViewCallbackHandler");
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

	private Telegram.Bot.Types.ReplyMarkups.InlineKeyboardMarkup GetProfileKeyboard(string languageCode)
	{
		var loc = _localizationService;

		return new Telegram.Bot.Types.ReplyMarkups.InlineKeyboardMarkup(new[]
		{
			new[]
			{
				Telegram.Bot.Types.ReplyMarkups.InlineKeyboardButton.WithCallbackData(
					loc.GetMessage("change_language", languageCode),
					DigiStore.TgBot.Application.Constants.CallbackData.LanguageChangePrefix + "select")
			},
			new[]
			{
				Telegram.Bot.Types.ReplyMarkups.InlineKeyboardButton.WithCallbackData(
					loc.GetMessage("main_menu", languageCode),
					DigiStore.TgBot.Application.Constants.CallbackData.MenuMain)
			},
		});
	}
}
