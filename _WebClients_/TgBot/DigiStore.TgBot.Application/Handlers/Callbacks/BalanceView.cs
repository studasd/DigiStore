using CSharpFunctionalExtensions;
using DigiStore.Enums;
using DigiStore.SharedKernel;
using DigiStore.TgBot.Application.Constants;
using DigiStore.TgBot.Application.Handlers.Adstracts;
using DigiStore.TgBot.Application.Interfaces.Services;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
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
		ITelegramBotClient botClient,
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

		try
		{
			await _botClient.EditMessageText(
				callbackQuery.Message.Chat.Id,
				callbackQuery.Message.MessageId,
				text,
				parseMode: ParseMode.Html,
				replyMarkup: keyboard,
				cancellationToken: token);

			await _botClient.AnswerCallbackQuery(callbackQuery.Id, cancellationToken: token);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error in BalanceViewCallbackHandler");
			await AnswerCallbackQueryWithError(callbackQuery.Id, LanguageCodes.en, token);
			return Error.Failure("callback.balance.error", "Error in BalanceViewCallbackHandler");
		}

		return Result.Success<Error>();
	}
}
