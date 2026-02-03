using CSharpFunctionalExtensions;
using DigiStore.Enums;
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
/// Обработчик колбэка просмотра баланса
/// </summary>
public class BalanceCallback : BaseHandler, ICallbackHandler
{
	public const string CallbackData = Constants.CallbackData.BalanceCallback;
	public const bool IsPrefix = false;

	private readonly IWalletService _walletService;
	private readonly ISessionService _sessionService;
	private readonly ILogger<BalanceCallback> _logger;

	

	public BalanceCallback(
		IBotAPIClient botClient,
		IWalletService walletService,
		ISessionService sessionService,
		ILocalizationService localizationService,
		ILogger<BalanceCallback> logger)
		: base(botClient, localizationService)
	{
		_walletService = walletService;
		_sessionService = sessionService;
		_logger = logger;
	}

	public async Task<UnitResult<Error>> HandleAsync(CallbackQuery callbackQuery, CancellationToken token = default)
	{
		// Handle balance view callback

		if (callbackQuery.Message == null)
			return Error.Failure("callback.balance.nomessage", "No message in BalanceViewCallbackHandler");

		
		var telegramId = callbackQuery.From.Id;
		var sessionResult = await _sessionService.GetSessionAsync(telegramId, token);

		if(sessionResult.IsFailure)
		{
			_logger.LogError("Failed to get session in BalanceViewCallbackHandler: {Error}", sessionResult.Error.GetMessage());
			await AnswerCallbackQueryWithError(callbackQuery.Id, LanguageCodes.en, token);
			return sessionResult.Error;
		}

		var session = sessionResult.Value;
		if (session?.UserId == null)
			return Error.Failure("callback.balance.noserid", "No UserId in BalanceViewCallbackHandler"); ;

		var langCode = session.LangCode;

		var walletResult = await _walletService.GetBalanceAsync(session.UserId, token);

		if (walletResult.IsFailure)
		{
			_logger.LogError("Failed to get wallet in BalanceViewCallbackHandler: {Error}", walletResult.Error.GetMessage());
			await AnswerCallbackQueryWithError(callbackQuery.Id, langCode, token);
			return walletResult.Error;
		}

		var wallet = walletResult.Value!;

///LocalKeys.Templates.BalanceView
//💰 БАЛАНС КОШЕЛЬКА

//Текущий баланс: {{balance}} {{currency}}
//📊 Всего пополнено: {{total_deposited}} {{currency}}
//📤 Всего снято: {{total_withdrawn}} {{currency}}

//Пополнить баланс через:


		var model = new
		{
			balance = (int)wallet.Balance,
			total_deposited = wallet.TotalDeposited,
			total_withdrawn = wallet.TotalWithdrawn,
			currency = wallet.Currency
		};

		var text = _localService.GetMessage(LocalKeys.Templates.BalanceCallback, langCode, model);


		var keyboard = GetBackToMainMenuKeyboard(langCode);

		var editResult = await _botClient.EditMessageTextAsync(
			callbackQuery.Message.Chat.Id,
			callbackQuery.Message.MessageId,
			text,
			parseMode: ParseMode.Html,
			replyMarkup: keyboard,
			cancellationToken: token);

		if (editResult.IsFailure)
		{
			_logger.LogError("Failed to edit message in BalanceViewCallbackHandler: {Error}", editResult.Error.GetMessage());
			return await AnswerCallbackQueryWithError(callbackQuery.Id, LanguageCodes.en, token);
		}

		return await _botClient.AnswerCallbackQueryAsync(callbackQuery.Id, cancellationToken: token);
	}
}
