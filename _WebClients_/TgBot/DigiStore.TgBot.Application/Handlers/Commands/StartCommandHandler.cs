using DigiStore.TgBot.Application.Constants;
using DigiStore.TgBot.Application.Handlers.Attributes;
using DigiStore.TgBot.Application.Interfaces;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace DigiStore.TgBot.Application.Handlers.Commands;

/// <summary>
/// Обработчик команды /start
/// </summary>
[Command(BotCommands.Start)]
public class StartCommandHandler : ICommandHandler
{
	private readonly ITelegramUserService _userService;
	private readonly ITelegramSessionService _sessionService;
	private readonly ILocalizationService _localizationService;
	private readonly ILogger<StartCommandHandler> _logger;

	public StartCommandHandler(
		ITelegramUserService userService,
		ITelegramSessionService sessionService,
		ILocalizationService localizationService,
		ILogger<StartCommandHandler> logger)
	{
		_userService = userService;
		_sessionService = sessionService;
		_localizationService = localizationService;
		_logger = logger;
	}

	public async Task HandleAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken = default)
	{
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
				await SendErrorMessage(botClient, message.Chat.Id, "Failed to initialize user account", cancellationToken);
				return;
			}

			var user = userResult.Value!;

			// Get or create session
			var session = await _sessionService.GetOrCreateSessionAsync(telegramId, cancellationToken);
			session.UserId = user.Id;
			session.LanguageCode = user.LanguageCode;

			// Send greeting and language selection
			await SendLanguageSelection(botClient, message.Chat.Id, session.LanguageCode, isStartCommand: true, cancellationToken);

			// Update session - waiting for language selection
			session.SetState(BotState.AwaitingLanguageSelection);
			await _sessionService.UpdateSessionAsync(session, cancellationToken);

			_logger.LogInformation("User initialized: TelegramId: {TelegramId}, UserId: {UserId}",
				telegramId, user.Id);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error in StartCommandHandler");
			await SendErrorMessage(botClient, message.Chat.Id, "An error occurred", cancellationToken);
		}
	}

	private async Task SendLanguageSelection(
		ITelegramBotClient botClient,
		long chatId,
		string currentLanguage,
		bool isStartCommand,
		CancellationToken ct)
	{
		var languages = _localizationService.GetLanguages();
		var buttons = new List<List<InlineKeyboardButton>>();

		foreach (var lang in languages)
		{
			var callbackData = isStartCommand
				? $"{CallbackData.LanguagePrefix}{lang.Key}"
				: $"{CallbackData.LanguageChangePrefix}{lang.Key}";

			buttons.Add(new List<InlineKeyboardButton>
			{
				InlineKeyboardButton.WithCallbackData(lang.Value, callbackData)
			});
		}

		var keyboard = new InlineKeyboardMarkup(buttons);
		string text;

		if (isStartCommand)
		{
			text = $"{_localizationService.GetMessage("greeting", currentLanguage)}\n\n" +
				   $"{_localizationService.GetMessage("select_language", currentLanguage)}";
		}
		else
		{
			text = _localizationService.GetMessage("select_language", currentLanguage);
		}

		await botClient.SendMessage(chatId, text, replyMarkup: keyboard, cancellationToken: ct);
	}

	private async Task SendErrorMessage(
		ITelegramBotClient botClient,
		long chatId,
		string error,
		CancellationToken ct)
	{
		await botClient.SendMessage(chatId, $"❌ {error}", cancellationToken: ct);
	}
}

