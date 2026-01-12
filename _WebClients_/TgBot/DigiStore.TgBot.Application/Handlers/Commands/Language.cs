using DigiStore.TgBot.Application.Constants;
using DigiStore.TgBot.Application.Handlers;
using DigiStore.TgBot.Application.Interfaces.Services;
using DigiStore.UserService.Contracts.Enums;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace DigiStore.TgBot.Application.Handlers.Commands;

/// <summary>
/// Обработчик команды /language
/// </summary>
public class Language : BaseHandler, ICommandHandler
{
	public const string Command = BotCommands.Language;
	
	private readonly ISessionService _sessionService;
	private readonly ILogger<Language> _logger;

	string ICommandHandler.Command => Command;

	public Language(
		ITelegramBotClient botClient,
		ISessionService sessionService,
		ILocalizationService localizationService,
		ILogger<Language> logger)
		: base(botClient, localizationService)
	{
		_sessionService = sessionService;
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
				await SendErrorMessage(chatId, _localizationService.GetMessage("session_expired", LanguageCodes.en), cancellationToken);
				return;
			}

			var currentLanguage = session.LangCode;

			// Send language selection
			await SendLanguageSelection(chatId, currentLanguage, isStartCommand: false, cancellationToken);

			// Update session
			session.SetState(BotState.LanguageChangeAwaiting);
			await _sessionService.UpdateSessionAsync(session, cancellationToken);

			_logger.LogInformation("Language selection sent for user: {TelegramId}", telegramId);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error in LanguageCommandHandler");
			await SendErrorMessage(message.Chat.Id, "An error occurred", cancellationToken);
		}
	}
}
