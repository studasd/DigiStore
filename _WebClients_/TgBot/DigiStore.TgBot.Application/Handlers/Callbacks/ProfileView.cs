using CSharpFunctionalExtensions;
using DigiStore.Enums;
using DigiStore.SharedKernel;
using DigiStore.TgBot.Application.Handlers.Adstracts;
using DigiStore.TgBot.Application.Interfaces;
using DigiStore.TgBot.Application.Interfaces.Services;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace DigiStore.TgBot.Application.Handlers.Callbacks;

/// <summary>
/// Обработчик колбэка просмотра профиля
/// </summary>
public class ProfileView : BaseHandler, ICallbackQueryHandler
{
	public const string CallbackData = Constants.CallbackData.ProfileView;
	public const bool IsPrefix = false;
	
	private readonly IProfileService _profileService;
	private readonly ISessionService _sessionService;
	private readonly ILogger<ProfileView> _logger;


	public ProfileView(
		IBotAPIClient botClient,
		IProfileService profileService,
		ISessionService sessionService,
		ILocalizationService localizationService,
		ILogger<ProfileView> logger)
		: base(botClient, localizationService)
	{
		_profileService = profileService;
		_sessionService = sessionService;
		_logger = logger;
	}

	public async Task<UnitResult<Error>> HandleAsync(CallbackQuery callbackQuery, CancellationToken token = default)
	{
		// Handle profile view callback

		if (callbackQuery.Message == null)
			return Error.Failure("callback.profile.nomessage", "No message in ProfileViewCallbackHandler");
		

		var telegramId = callbackQuery.From.Id;
		var sessionResult = await _sessionService.GetSessionAsync(telegramId, token);

		if (sessionResult.IsFailure)
			return sessionResult.Error;

		var session = sessionResult.Value;
		var languageCode = session.LangCode;

		var profileResult = await _profileService.GetFullProfileAsync(
			session.UserId,
			session.TelegramId,
			token);

		if (profileResult.IsFailure)
		{
			await AnswerCallbackQueryWithError(callbackQuery.Id, languageCode, token);
			return profileResult.Error;
		}

		var profile = profileResult.Value!;
		var profileText = _profileService.FormatProfileText(profile, languageCode);
		var keyboard = GetProfileKeyboard(languageCode);


		var editResult = await _botClient.EditMessageTextAsync(
			callbackQuery.Message.Chat.Id,
			callbackQuery.Message.MessageId,
			profileText,
			parseMode: ParseMode.Html,
			replyMarkup: keyboard,
			cancellationToken: token);

		if (editResult.IsFailure)
		{
			_logger.LogError("Error in ProfileViewCallbackHandler");
			return await AnswerCallbackQueryWithError(callbackQuery.Id, LanguageCodes.en, token);
		}

		return await _botClient.AnswerCallbackQueryAsync(callbackQuery.Id, cancellationToken: token);
	}
}
