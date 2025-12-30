using DigiStore.TgBot.Application.Constants;
using DigiStore.TgBot.Domain;
using DigiStore.TgBot.Application.Handlers;
using DigiStore.TgBot.Application.Interfaces;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace DigiStore.TgBot.Application.Handlers.Commands;

/// <summary>
/// Обработчик команды /profile
/// </summary>
public class ProfileCommandHandler : BaseHandler, ICommandHandler
{
	public const string Command = BotCommands.Profile;
	
	private readonly ITelegramProfileService _profileService;
	private readonly ITelegramSessionService _sessionService;
	private readonly ILogger<ProfileCommandHandler> _logger;

	string ICommandHandler.Command => Command;

	public ProfileCommandHandler(
		ITelegramBotClient botClient,
		ITelegramProfileService profileService,
		ITelegramSessionService sessionService,
		ILocalizationService localizationService,
		ILogger<ProfileCommandHandler> logger)
		: base(botClient, localizationService)
	{
		_profileService = profileService;
		_sessionService = sessionService;
		_logger = logger;
	}

	public async Task HandleAsync(Message message, CancellationToken cancellationToken = default)
	{
		// Handle /profile command - show user profile with balance

		try
		{
			var telegramId = message.From!.Id;
			var chatId = message.Chat.Id;

			_logger.LogInformation("Profile command from Telegram ID: {TelegramId}", telegramId);

			// Get session
			var session = await _sessionService.GetSessionAsync(telegramId, cancellationToken);
			if (session?.UserId == null)
			{
				await SendErrorMessage(chatId, _localizationService.GetMessage("session_expired", "en"), cancellationToken);
				return;
			}

			var userId = session.UserId.Value;
			var languageCode = session.LanguageCode ?? "en";

			// Get full profile
			var profileResult = await _profileService.GetFullProfileAsync(userId, telegramId, cancellationToken);
			if (!profileResult.IsSuccess)
			{
				await SendErrorMessage(chatId, _localizationService.GetMessage("error_occurred", languageCode), cancellationToken);
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

			await _botClient.SendMessage(
				chatId,
				profileText,
				parseMode: ParseMode.Html,
				replyMarkup: keyboard,
				cancellationToken: cancellationToken);

			_logger.LogInformation("Profile sent for user: {UserId}, TelegramId: {TelegramId}", userId, telegramId);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error in ProfileCommandHandler");
			await SendErrorMessage(message.Chat.Id, "An error occurred", cancellationToken);
		}
	}
}
