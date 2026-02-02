using CSharpFunctionalExtensions;
using DigiStore.TgBot.Application.Constants;
using DigiStore.TgBot.Application.Handlers.Adstracts;
using DigiStore.TgBot.Application.Interfaces.Services;
using Microsoft.Extensions.Logging;
using StudCoreKit.SharedKernel;
using StudTgBotApi.Contracts.Interfaces;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace DigiStore.TgBot.Application.Handlers.Balances;

/// <summary>
/// Обработчик команды /balance
/// </summary>
public class BalanceCommand : BaseHandler, ICommandHandler
{
	public const string Command = BotCommands.Balance;

	private readonly IProfileService _profileService;
	private readonly ISessionService _sessionService;
	private readonly ILogger<BalanceCommand> _logger;


	public BalanceCommand(
		IBotAPIClient botClient,
		IProfileService profileService,
		ISessionService sessionService,
		ILocalizationService localizationService,
		ILogger<BalanceCommand> logger)
		: base(botClient, localizationService)
	{
		_profileService = profileService;
		_sessionService = sessionService;
		_logger = logger;
	}

	public async Task<UnitResult<Error>> HandleAsync(Message message, CancellationToken token = default)
	{
		// Handle /balance command - show wallet info
		
		var telegramId = message.From!.Id;
		var chatId = message.Chat.Id;

		// Get session
		var sessionResult = await _sessionService.GetSessionAsync(telegramId, token);
		if (sessionResult.IsFailure)
		{
			await SendErrorMessage(chatId, "Session expired", token);
			return sessionResult.Error;
		}

		var session = sessionResult.Value;
		var langCode = session.LangCode;

		var profileResult = await _profileService.GetFullProfileAsync(session.UserId, telegramId, token);
		if (profileResult.IsFailure)
		{
			await SendErrorMessage(chatId, _localService.GetMessage(LocalKeys.Errors.Occurred, langCode), token);
			return profileResult;
		}

		var profile = profileResult.Value!;


///LocalKeys.Templates.BalanceView
//💰 БАЛАНС КОШЕЛЬКА

//Текущий баланс: <b>{{balance}} {{currency}}</b>
//🔗 Привязанные аккаунты
//👤 Telegram: {{username}}


		var model = new
		{
			balance = profile.Balance,
			currency = profile.Currency,
			username = profile.Username
		};

		var text = _localService.GetMessage(LocalKeys.Templates.Balance, langCode, model);


		var keyboard = GetBackToMainMenuKeyboard(langCode);

		var sendResult = await _botClient.SendMessageAsync(
			chatId,
			text,
			parseMode: ParseMode.Html,
			replyMarkup: keyboard,
			cancellationToken: token);

		if (sendResult.IsFailure)
		{
			_logger.LogError("Failed to send balance message to chat {ChatId}", chatId);
			return await SendErrorMessage(chatId, _localService.GetMessage(LocalKeys.Errors.Occurred, langCode), token);
		}

		return Result.Success<Error>();
	}
}
