using CSharpFunctionalExtensions;
using DigiStore.Enums;
using DigiStore.SharedKernel;
using DigiStore.TgBot.Application.Constants;
using DigiStore.TgBot.Application.Handlers.Adstracts;
using DigiStore.TgBot.Application.Interfaces;
using DigiStore.TgBot.Application.Interfaces.Services;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace DigiStore.TgBot.Application.Handlers.Callbacks;

/// <summary>
/// Обработчик колбэка просмотра баланса
/// </summary>
public class BalanceView : BaseHandler, ICallbackQueryHandler
{
	public const string CallbackData = Constants.CallbackData.BalanceView;
	public const bool IsPrefix = false;

	private readonly IWalletService _walletService;
	private readonly ISessionService _sessionService;
	private readonly ILogger<BalanceView> _logger;

	

	public BalanceView(
		IBotAPIClient botClient,
		IWalletService walletService,
		ISessionService sessionService,
		ILocalizationService localizationService,
		ILogger<BalanceView> logger)
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
		var text = $@"
💰 {_localService.GetMessage(LocalKeys.Balances.Info, langCode)}

{_localService.GetMessage(LocalKeys.Balances.CurrentBalance, langCode)}: <b>{wallet.Balance:F2} {wallet.Currency}</b>
📊 {_localService.GetMessage(LocalKeys.Balances.TotalDeposited, langCode)}: {wallet.TotalDeposited:F2} {wallet.Currency}
📤 {_localService.GetMessage(LocalKeys.Balances.TotalWithdrawn, langCode)}: {wallet.TotalWithdrawn:F2} {wallet.Currency}

{_localService.GetMessage(LocalKeys.Balances.TopUpBalance, langCode)}:
";

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
