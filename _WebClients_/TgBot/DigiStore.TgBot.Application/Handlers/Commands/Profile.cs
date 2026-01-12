using DigiStore.TgBot.Application.Constants;
using DigiStore.TgBot.Application.Interfaces.Services;
using DigiStore.TgBot.Domain;
using DigiStore.TgBot.Domain.ValueObjects;
using DigiStore.UserService.Contracts.Enums;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace DigiStore.TgBot.Application.Handlers.Commands;

/// <summary>
/// Обработчик команды /profile
/// </summary>
public class Profile : BaseHandler, ICommandHandler
{
	public const string Command = BotCommands.Profile;
	
	private readonly IProfileService _profileService;
	private readonly ISessionService _sessionService;
	private readonly ILogger<Profile> _logger;

	string ICommandHandler.Command => Command;

	public Profile(
		ITelegramBotClient botClient,
		IProfileService profileService,
		ISessionService sessionService,
		ILocalizationService localizationService,
		ILogger<Profile> logger)
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
				await SendErrorMessage(chatId, _localizationService.GetMessage("session_expired", LanguageCodes.en), cancellationToken);
				return;
			}

			var userId = session.UserId;
			var languageCode = session.LangCode;

			// Get full profile
			var profileResult = await _profileService.GetFullProfileAsync(userId, telegramId, cancellationToken);
			if (!profileResult.IsSuccess)
			{
				await SendErrorMessage(chatId, _localizationService.GetMessage("error_occurred", languageCode), cancellationToken);
				return;
			}

			var profile = profileResult.Value!;

			// Cache profile in session
			session.CachedProfile = new CachedUserProfileVO
			{
				UserId = profile.UserId,
				TelegramId = profile.TelegramId,
				Email = profile.Email,
				FirstName = profile.FullName.Split(' ').FirstOrDefault() ?? string.Empty,
				LastName = profile.FullName.Split(' ').LastOrDefault() ?? string.Empty,
				Username = profile.Username,
				LangCode = profile.LangCode,
				IsActive = profile.IsActive,
				Roles = profile.Roles,
				Balance = profile.Balance,
				Currency = profile.Currency,
				CreatedAt = profile.CreatedAt,
				UpdatedAt = profile.UpdatedAt
			};

			session.SetState(BotState.ProfileViewing);
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
