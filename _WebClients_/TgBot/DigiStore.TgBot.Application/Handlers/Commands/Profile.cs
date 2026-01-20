using CSharpFunctionalExtensions;
using DigiStore.Enums;
using DigiStore.SharedKernel;
using DigiStore.TgBot.Application.Constants;
using DigiStore.TgBot.Application.Handlers.Adstracts;
using DigiStore.TgBot.Application.Interfaces;
using DigiStore.TgBot.Application.Interfaces.Services;
using DigiStore.TgBot.Domain.ValueObjects;
using Microsoft.Extensions.Logging;
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


	public Profile(
		IBotAPIClient botClient,
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

	public async Task<UnitResult<Error>> HandleAsync(Message message, CancellationToken token = default)
	{
		// Handle /profile command - show user profile with balance
		
		var telegramId = message.From!.Id;
		var chatId = message.Chat.Id;

		_logger.LogInformation("Profile command from Telegram ID: {TelegramId}", telegramId);

		// Get session
		var sessionResult = await _sessionService.GetSessionAsync(telegramId, token);
		if (sessionResult.IsFailure)
		{
			await SendErrorMessage(chatId, _localService.GetMessage(LocalKeys.Errors.SessionExpired, LanguageCodes.en), token);
			return sessionResult.Error;
		}

		var session = sessionResult.Value!;
		var userId = session.UserId;
		var languageCode = session.LangCode;

		// Get full profile
		var profileResult = await _profileService.GetFullProfileAsync(userId, telegramId, token);
		if (profileResult.IsFailure)
		{
			await SendErrorMessage(chatId, _localService.GetMessage(LocalKeys.Errors.Occurred, languageCode), token);
			return sessionResult.Error;
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
		await _sessionService.UpdateSessionAsync(session, token);

		// Format and send profile
		var profileText = _profileService.FormatProfileText(profile, languageCode);
		var keyboard = GetProfileKeyboard(languageCode);

		var sendResult = await _botClient.SendMessageAsync(
				chatId,
				profileText,
				parseMode: ParseMode.Html,
				replyMarkup: keyboard,
				cancellationToken: token);

		if(sendResult.IsFailure) 
		{ 
			await SendErrorMessage(message.Chat.Id, "An error occurred", token);
			return Error.Failure("command.handler.profile", "Error in ProfileCommandHandler");
		}

		_logger.LogInformation("Profile sent for user: {UserId}, TelegramId: {TelegramId}", userId, telegramId);
		return Result.Success<Error>();
	}
}
