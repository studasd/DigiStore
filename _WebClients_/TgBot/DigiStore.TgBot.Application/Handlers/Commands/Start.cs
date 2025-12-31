using DigiStore.TgBot.Application.Constants;
using DigiStore.TgBot.Application.Handlers;
using DigiStore.TgBot.Application.Interfaces;
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
	
	private readonly ITelegramUserService _userService;
	private readonly ITelegramSessionService _sessionService;
	private readonly ILogger<Start> _logger;

	string ICommandHandler.Command => Command;

	public Start(
		ITelegramBotClient botClient,
		ITelegramUserService userService,
		ITelegramSessionService sessionService,
		ILocalizationService localizationService,
		ILogger<Start> logger)
		: base(botClient, localizationService)
	{
		_userService = userService;
		_sessionService = sessionService;
		_logger = logger;
	}

	public async Task HandleAsync(Message message, CancellationToken cancellationToken = default)
	{
		// Handle /start command
		// 1. Check if user exists
		// 2. Create if not
		// 3. Ask for language
		// 4. Show profile after language selection
		
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

			// Send greeting and language selection
			await SendLanguageSelection(message.Chat.Id, session.LanguageCode, isStartCommand: true, cancellationToken);

			// Update session - waiting for language selection
			session.SetState(BotState.AwaitingLanguageSelection);
			await _sessionService.UpdateSessionAsync(session, cancellationToken);

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
