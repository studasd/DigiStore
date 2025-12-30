using DigiStore.TgBot.Application.Constants;
using DigiStore.TgBot.Domain;
using DigiStore.TgBot.Application.Handlers.Attributes;
using DigiStore.TgBot.Application.Interfaces;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace DigiStore.TgBot.Application.Handlers.Callbacks;

/// <summary>
/// Обработчик колбэка выбора языка из команды /start
/// </summary>
[CallbackQuery(CallbackData.LanguagePrefix, IsPrefix = true)]
public class LanguageSelectionCallbackHandler : ICallbackQueryHandler
{
	private readonly ITelegramProfileService _profileService;
	private readonly ITelegramSessionService _sessionService;
	private readonly ILocalizationService _localizationService;
	private readonly ILogger<LanguageSelectionCallbackHandler> _logger;

	public LanguageSelectionCallbackHandler(
		ITelegramProfileService profileService,
		ITelegramSessionService sessionService,
		ILocalizationService localizationService,
		ILogger<LanguageSelectionCallbackHandler> logger)
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

			var languageCode = callbackQuery.Data.Replace(CallbackData.LanguagePrefix, "");

			if (!session.UserId.HasValue)
				return;

			_logger.LogInformation(
				"Language selected from /start: {LanguageCode}, UserId: {UserId}",
				languageCode, session.UserId);

			// Update user language in UserService
			var updateResult = await _profileService.UpdateUserLanguageAsync(
				session.UserId.Value,
				languageCode,
				cancellationToken);

			if (!updateResult.IsSuccess)
			{
				await botClient.AnswerCallbackQuery(
					callbackQuery.Id,
					_localizationService.GetMessage("error_occurred", languageCode),
					showAlert: true,
					cancellationToken: cancellationToken);
				return;
			}

			// Update session
			session.LanguageCode = languageCode;
			session.SetState(BotState.LanguageSelected);
			await _sessionService.UpdateSessionAsync(session, cancellationToken);

			// Get full profile
			var profileResult = await _profileService.GetFullProfileAsync(
				session.UserId.Value,
				session.TelegramId,
				cancellationToken);

			if (!profileResult.IsSuccess)
			{
				await botClient.AnswerCallbackQuery(
					callbackQuery.Id,
					_localizationService.GetMessage("error_occurred", languageCode),
					showAlert: true,
					cancellationToken: cancellationToken);
				return;
			}

			var profile = profileResult.Value!;

			// Cache profile in session
			session.CachedProfile = new CachedUserProfile
			{
				UserId = profile.UserId,
				TelegramId = profile.TelegramId,
				Email = profile.Email,
				FirstName = profile.FullName.Split(' ').FirstOrDefault() ?? string.Empty,
				LastName = profile.FullName.Split(' ').LastOrDefault() ?? string.Empty,
				TelegramUsername = profile.TelegramUsername,
				LanguageCode = profile.LanguageCode,
				IsActive = profile.IsActive,
				Roles = profile.Roles,
				Balance = profile.Balance,
				Currency = profile.Currency,
				CreatedAt = profile.CreatedAt,
				UpdatedAt = profile.UpdatedAt
			};

			session.SetState(BotState.ViewingProfile);
			await _sessionService.UpdateSessionAsync(session, cancellationToken);

			// Format and send profile
			var profileText = _profileService.FormatProfileText(profile, languageCode);
			var keyboard = GetProfileKeyboard(languageCode);

			await botClient.EditMessageText(
				callbackQuery.Message.Chat.Id,
				callbackQuery.Message.MessageId,
				profileText,
				parseMode: ParseMode.Html,
				replyMarkup: keyboard,
				cancellationToken: cancellationToken);

			await botClient.AnswerCallbackQuery(callbackQuery.Id, cancellationToken: cancellationToken);

			_logger.LogInformation(
				"Profile shown after language selection for user: {UserId}",
				session.UserId);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error in LanguageSelectionCallbackHandler");
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

	private Telegram.Bot.Types.ReplyMarkups.InlineKeyboardMarkup GetProfileKeyboard(string languageCode)
	{
		var loc = _localizationService;
		return new Telegram.Bot.Types.ReplyMarkups.InlineKeyboardMarkup(new[]
		{
			new[]
			{
				Telegram.Bot.Types.ReplyMarkups.InlineKeyboardButton.WithCallbackData(
					loc.GetMessage("change_language", languageCode),
					CallbackData.LanguageChangePrefix + "select")
			},
			new[]
			{
				Telegram.Bot.Types.ReplyMarkups.InlineKeyboardButton.WithCallbackData(
					loc.GetMessage("main_menu", languageCode),
					CallbackData.MenuMain)
			},
		});
	}
}

