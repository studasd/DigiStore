using DigiStore.TgBot.Application.Constants;
using DigiStore.TgBot.Application.Handlers;
using DigiStore.TgBot.Application.Interfaces.Services;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using System.Threading.Tasks;

namespace DigiStore.TgBot.Application.Handlers.Commands;

/// <summary>
/// Обработчик команды /start
/// </summary>
public class Start : BaseHandler, ICommandHandler
{
	public const string Command = BotCommands.Start;
	
	private readonly ITelegramUserService _userService;
	private readonly ITelegramSessionService _sessionService;
	private readonly ITelegramProfileService _profileService;
	private readonly ILogger<Start> _logger;

	string ICommandHandler.Command => Command;

	public Start(
		ITelegramBotClient botClient,
		ITelegramUserService userService,
		ITelegramSessionService sessionService,
		ITelegramProfileService profileService,
		ILocalizationService localizationService,
		ILogger<Start> logger)
		: base(botClient, localizationService)
	{
		_userService = userService;
		_sessionService = sessionService;
		_profileService = profileService;
		_logger = logger;
	}

	public async Task HandleAsync(Message message, CancellationToken cancellationToken = default)
	{
		// Handle /start command
		// 1. Check if user exists
		// 2. Create if not
		// 3. Ask for language (only for newly created users)
		// 4. Show profile immediately if user already existed
		
		try
		{
			var telegramId = message.From!.Id;
			var username = message.From.Username;
			var firstName = message.From.FirstName;
			var lastName = message.From.LastName;
			var defaultLanguage = message.From.LanguageCode ?? "en";

			_logger.LogInformation("Start command from Telegram ID: {TelegramId}, Name: {FirstName} {LastName}",
				telegramId, firstName, lastName);

			// Get or create user
			var userResult = await _userService.GetOrCreateUserAsync(
				telegramId,
				username,
				firstName,
				lastName,
				defaultLanguage,
				cancellationToken);

			if (!userResult.IsSuccess)
			{
				await SendErrorMessage(message.Chat.Id, "Failed to initialize user account", cancellationToken);
				return;
			}

			var user = userResult.Value!;

			// Get or create session
			var session = await _sessionService.GetOrCreateSessionAsync(telegramId, cancellationToken);
			session.UserId = user.Id;
			session.LanguageCode = user.LanguageCode;

			// If user was just created -> ask for language
			if (user.IsNew)
			{
				// Send greeting and language selection
				await SendLanguageSelection(message.Chat.Id, session.LanguageCode, isStartCommand: true, cancellationToken);

				// Update session - waiting for language selection
				session.SetState(BotState.LanguageSelectionAwaiting);
				await _sessionService.UpdateSessionAsync(session, cancellationToken);
			}
			else
			{
				// Existing user - show profile immediately
				var profileResult = await _profileService.GetFullProfileAsync(user.Id, telegramId, cancellationToken);
				if (!profileResult.IsSuccess)
				{
					await SendErrorMessage(message.Chat.Id, _localizationService.GetMessage("error_occurred", session.LanguageCode ?? "en"), cancellationToken);
					return;
				}

				var profile = profileResult.Value!;

				// Cache profile in session
				session.CachedProfile = new DigiStore.TgBot.Domain.CachedUserProfile
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

				session.SetState(BotState.ProfileViewing);
				await _sessionService.UpdateSessionAsync(session, cancellationToken);

				var profileText = _profileService.FormatProfileText(profile, session.LanguageCode ?? "en");
				var keyboard = GetProfileKeyboard(session.LanguageCode ?? "en");

				await _botClient.SendMessage(
					message.Chat.Id,
					profileText,
					parseMode: Telegram.Bot.Types.Enums.ParseMode.Html,
					replyMarkup: keyboard,
					cancellationToken: cancellationToken);
			}

			_logger.LogInformation("User initialized: TelegramId: {TelegramId}, UserId: {UserId}",
				telegramId, user.Id);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error in StartCommandHandler");
			await SendErrorMessage(message.Chat.Id, "An error occurred", cancellationToken);
		}
	}
}
