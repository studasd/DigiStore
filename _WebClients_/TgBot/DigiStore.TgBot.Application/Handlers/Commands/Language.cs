using CSharpFunctionalExtensions;
using DigiStore.Enums;
using DigiStore.SharedKernel;
using DigiStore.TgBot.Application.Constants;
using DigiStore.TgBot.Application.Handlers.Adstracts;
using DigiStore.TgBot.Application.Interfaces.Services;
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

	public async Task<UnitResult<Error>> HandleAsync(Message message, CancellationToken token = default)
	{
		// Handle /language command - change language

		var telegramId = message.From!.Id;
		var chatId = message.Chat.Id;

		_logger.LogInformation("Language command from Telegram ID: {TelegramId}", telegramId);

		// Get session
		var sessionResult = await _sessionService.GetSessionAsync(telegramId, token);
		if (sessionResult.IsFailure)
		{
			await SendErrorMessage(chatId, _localService.GetMessage(LocalKeys.Errors.SessionExpired, LanguageCodes.en), token);
			return sessionResult.Error;
		}

		var session = sessionResult.Value;
		var currentLanguage = session.LangCode;

		// Send language selection
		var sendLangResult = await SendLanguageSelection(chatId, currentLanguage, isStartCommand: false, token);

		if (sendLangResult.IsFailure)
		{
			return sendLangResult.Error;
		}

		// Update session
		session.SetState(BotState.LanguageChangeAwaiting);
		await _sessionService.UpdateSessionAsync(session, token);

		_logger.LogInformation("Language selection sent for user: {TelegramId}", telegramId);
		return Result.Success<Error>();
	}
}
