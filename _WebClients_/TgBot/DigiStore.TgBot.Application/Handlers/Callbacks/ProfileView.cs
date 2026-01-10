using DigiStore.TgBot.Application.Constants;
using DigiStore.TgBot.Application.Handlers;
using DigiStore.TgBot.Application.Interfaces.Services;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace DigiStore.TgBot.Application.Handlers.Callbacks;

/// <summary>
/// Обработчик колбэка просмотра профиля
/// </summary>
public class ProfileView : BaseHandler, ICallbackQueryHandler
{
	public const string CallbackData = DigiStore.TgBot.Application.Constants.CallbackData.ProfileView;
	public const bool IsPrefix = false;
	
	private readonly ITelegramProfileService _profileService;
	private readonly ITelegramSessionService _sessionService;
	private readonly ILogger<ProfileView> _logger;

	string ICallbackQueryHandler.CallbackData => CallbackData;
	bool ICallbackQueryHandler.IsPrefix => IsPrefix;

	public ProfileView(
		ITelegramBotClient botClient,
		ITelegramProfileService profileService,
		ITelegramSessionService sessionService,
		ILocalizationService localizationService,
		ILogger<ProfileView> logger)
		: base(botClient, localizationService)
	{
		_profileService = profileService;
		_sessionService = sessionService;
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
				session.UserId,
				session.TelegramId,
				cancellationToken);

			if (!profileResult.IsSuccess)
			{
				await AnswerCallbackQueryWithError(callbackQuery.Id, languageCode, cancellationToken);
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
			await AnswerCallbackQueryWithError(callbackQuery.Id, "en", cancellationToken);
		}
	}
}
