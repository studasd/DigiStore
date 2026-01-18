using CSharpFunctionalExtensions;
using DigiStore.Enums;
using DigiStore.SharedKernel;
using DigiStore.SharedKernel.Extensions;
using DigiStore.TgBot.Application.Constants;
using DigiStore.TgBot.Application.Handlers.Adstracts;
using DigiStore.TgBot.Application.Interfaces.Services;
using DigiStore.TgBot.Domain.ValueObjects;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace DigiStore.TgBot.Application.Handlers.Commands;

/// <summary>
/// Обработчик команды /start
/// </summary>
public class Start : BaseHandler, ICommandHandler
{
	public const string Command = BotCommands.Start;

	private readonly ITgUserService _userService;
	private readonly ISessionService _sessionService;
	private readonly IProfileService _profileService;
	private readonly ILogger<Start> _logger;


	public Start(
		ITelegramBotClient botClient,
		ITgUserService userService,
		ISessionService sessionService,
		IProfileService profileService,
		ILocalizationService localizationService,
		ILogger<Start> logger)
		: base(botClient, localizationService)
	{
		_userService = userService;
		_sessionService = sessionService;
		_profileService = profileService;
		_logger = logger;
	}


	public async Task<UnitResult<Error>> HandleAsync(Message message, CancellationToken token = default)
	{
		// Handle /start command
		// 1. Check if user exists
		// 2. Create if not
		// 3. Ask for language (only for newly created users)
		// 4. Show profile immediately if user already existed
		
		
		var telegramId = message.From!.Id;
		var username = message.From.Username;
		var firstName = message.From.FirstName;
		var lastName = message.From.LastName;
		var defaultLanguage = message.From.LanguageCode.ParseEnum<LanguageCodes>().Value;

		_logger.LogInformation("Start command from Telegram ID: {TelegramId}, Name: {FirstName} {LastName}",
			telegramId, firstName, lastName);

		// Get or create user
		var userResult = await _userService.GetOrCreateUserAsync(
			telegramId,
			username,
			firstName,
			lastName,
			defaultLanguage,
			token);

		if (userResult.IsFailure)
		{
			await SendErrorMessage(message.Chat.Id, "Failed to initialize user account", token);
			return userResult.Error;
		}

		var user = userResult.Value!;

		// Get or create session
		var sessionResult = await _sessionService.GetOrCreateSessionAsync(telegramId, token);

		if (sessionResult.IsFailure)
			return sessionResult.Error;
			
		var session = sessionResult.Value!;
		session.UserId = user.Id;
		session.LangCode = user.LangCode;

		// If user was just created -> ask for language
		if (user.IsNew)
		{
			// Send greeting and language selection
			var sendLangResult = await SendLanguageSelection(message.Chat.Id, session.LangCode, isStartCommand: true, token);
			if (sendLangResult.IsFailure)
			{
				return sendLangResult.Error;
			}

			// Update session - waiting for language selection
			session.SetState(BotState.LanguageSelectionAwaiting);
			await _sessionService.UpdateSessionAsync(session, token);
		}
		else
		{
			// Existing user - show profile immediately
			var profileResult = await _profileService.GetFullProfileAsync(user.Id, telegramId, token);
			
			if (profileResult.IsFailure)
			{
				await SendErrorMessage(message.Chat.Id, _localService.GetMessage(LocalKeys.Errors.Occurred, session.LangCode), token);
				return profileResult.Error;
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

			var profileText = _profileService.FormatProfileText(profile, session.LangCode);
			var keyboard = GetProfileKeyboard(session.LangCode);

			try
			{
				await _botClient.SendMessage(
					message.Chat.Id,
					profileText,
					parseMode: Telegram.Bot.Types.Enums.ParseMode.Html,
					replyMarkup: keyboard,
					cancellationToken: token);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error in StartCommandHandler");
				await SendErrorMessage(message.Chat.Id, "An error occurred", token);
				return Error.Failure("command.handler.start", "Error in StartCommandHandler");
			}

			_logger.LogInformation("User initialized: TelegramId: {TelegramId}, UserId: {UserId}",
				telegramId, user.Id);
		}

		return Result.Success<Error>();
	}
}
