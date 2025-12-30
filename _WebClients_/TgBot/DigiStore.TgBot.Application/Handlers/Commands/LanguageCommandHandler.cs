using DigiStore.TgBot.Application.Constants;
using DigiStore.TgBot.Application.Interfaces;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace DigiStore.TgBot.Application.Handlers.Commands;

/// <summary>
/// Обработчик команды /language
/// </summary>
public class LanguageCommandHandler : ICommandHandler
{
	public const string Command = BotCommands.Language;
	
	private readonly ITelegramBotClient _botClient;
	private readonly ITelegramSessionService _sessionService;
	private readonly ILocalizationService _localizationService;
	private readonly ILogger<LanguageCommandHandler> _logger;

	string ICommandHandler.Command => Command;

	public LanguageCommandHandler(
		ITelegramBotClient botClient,
		ITelegramSessionService sessionService,
		ILocalizationService localizationService,
		ILogger<LanguageCommandHandler> logger)
	{
		_botClient = botClient;
		_sessionService = sessionService;
		_localizationService = localizationService;
		_logger = logger;
	}

	public async Task HandleAsync(Message message, CancellationToken cancellationToken = default)
	{
		// Handle /language command - change language

		try
		{
			var telegramId = message.From!.Id;
			var chatId = message.Chat.Id;

			_logger.LogInformation("Language command from Telegram ID: {TelegramId}", telegramId);

			// Get session
			var session = await _sessionService.GetSessionAsync(telegramId, cancellationToken);
			if (session?.UserId == null)
			{
				await SendErrorMessage(chatId, _localizationService.GetMessage("session_expired", "en"), cancellationToken);
				return;
			}

			var currentLanguage = session.LanguageCode ?? "en";

			// Send language selection
			await SendLanguageSelection(chatId, currentLanguage, isStartCommand: false, cancellationToken);

			// Update session
			session.SetState(BotState.AwaitingLanguageChange);

			await _sessionService.UpdateSessionAsync(session, cancellationToken);

			_logger.LogInformation("Language selection sent for user: {TelegramId}", telegramId);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error in LanguageCommandHandler");
			await SendErrorMessage(message.Chat.Id, "An error occurred", cancellationToken);
		}
	}

	private async Task SendLanguageSelection(
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
		var text = _localizationService.GetMessage("select_language", currentLanguage);

		await _botClient.SendMessage(chatId, text, replyMarkup: keyboard, cancellationToken: ct);
	}

	private async Task SendErrorMessage(
		long chatId,
		string error,
		CancellationToken ct)
	{
		await _botClient.SendMessage(chatId, $"❌ {error}", cancellationToken: ct);
	}
}
