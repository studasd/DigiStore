using DigiStore.TgBot.Application.Constants;
using DigiStore.TgBot.Application.Handlers;
using DigiStore.TgBot.Application.Interfaces.Services;
using DigiStore.UserService.Contracts.Enums;
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

	public async Task HandleAsync(CallbackQuery callbackQuery, CancellationToken cancellationToken = default)
	{
		// Handle balance view callback

		if (callbackQuery.Message == null)
			return;

		try
		{
			var telegramId = callbackQuery.From.Id;
			var sessionResult = await _sessionService.GetSessionAsync(telegramId, cancellationToken);

			if(sessionResult.IsFailure)
			{
				await AnswerCallbackQueryWithError(callbackQuery.Id, LanguageCodes.en, cancellationToken);
				return;
			}

			var session = sessionResult.Value;
			if (session?.UserId == null)
				return;

			var langCode = session.LangCode;

			var walletResult = await _walletService.GetBalanceAsync(session.UserId, cancellationToken);

			if (!walletResult.IsSuccess)
			{
				await AnswerCallbackQueryWithError(callbackQuery.Id, langCode, cancellationToken);
				return;
			}

			var wallet = walletResult.Value!;
			var text = $@"
💰 {_localService.GetMessage(LocalKeys.Balances.Info, langCode)}

{_localService.GetMessage(LocalKeys.Balances.CurrentBalance, langCode)}: <b>{wallet.Balance:F2} {wallet.Currency}</b>
📊 {_localService.GetMessage(LocalKeys.Balances.TotalDeposited, langCode)}: {wallet.TotalDeposited:F2} {wallet.Currency}
📤 {_localService.GetMessage(LocalKeys.Balances.TotalWithdrawn, langCode)}: {wallet.TotalWithdrawn:F2} {wallet.Currency}
";

			var keyboard = GetBackToMainMenuKeyboard(langCode);

			await _botClient.EditMessageText(
				callbackQuery.Message.Chat.Id,
				callbackQuery.Message.MessageId,
				text,
				parseMode: ParseMode.Html,
				replyMarkup: keyboard,
				cancellationToken: cancellationToken);

			await _botClient.AnswerCallbackQuery(callbackQuery.Id, cancellationToken: cancellationToken);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error in BalanceViewCallbackHandler");
			await AnswerCallbackQueryWithError(callbackQuery.Id, LanguageCodes.en, cancellationToken);
		}
	}
}
