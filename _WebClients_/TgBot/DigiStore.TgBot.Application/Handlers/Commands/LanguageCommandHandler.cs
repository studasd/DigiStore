using DigiStore.TgBot.Application.Constants;
using DigiStore.TgBot.Application.Handlers.Attributes;
using DigiStore.TgBot.Application.Interfaces;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace DigiStore.TgBot.Application.Handlers.Commands;

/// <summary>
/// Обработчик команды /language
/// </summary>
[Command(BotCommands.Language)]
public class LanguageCommandHandler : ICommandHandler
{
	private readonly ITelegramSessionService _sessionService;
	private readonly ILocalizationService _localizationService;
	private readonly ILogger<LanguageCommandHandler> _logger;

	public LanguageCommandHandler(
		ITelegramSessionService sessionService,
		ILocalizationService localizationService,
		ILogger<LanguageCommandHandler> logger)
	{
		_sessionService = sessionService;
		_localizationService = localizationService;
		_logger = logger;
	}

	public async Task HandleAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken = default)
	{
		try
		{
			var telegramId = message.From!.Id;
			var chatId = message.Chat.Id;

			_logger.LogInformation("Language command from Telegram ID: {TelegramId}", telegramId);

			// Get session
			var session = await _sessionService.GetSessionAsync(telegramId, cancellationToken);
			if (session?.UserId == null)
			{
				await SendErrorMessage(botClient, chatId, _localizationService.GetMessage("session_expired", "en"), cancellationToken);
				return;
			}

			var currentLanguage = session.LanguageCode ?? "en";

			// Send language selection
			await SendLanguageSelection(botClient, chatId, currentLanguage, isStartCommand: false, cancellationToken);

			// Update session
			session.SetState(BotState.AwaitingLanguageChange);
			await _sessionService.UpdateSessionAsync(session, cancellationToken);

			_logger.LogInformation("Language selection sent for user: {TelegramId}", telegramId);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error in LanguageCommandHandler");
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
		var text = _localizationService.GetMessage("select_language", currentLanguage);

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

