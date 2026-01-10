using DigiStore.TgBot.Application.Constants;
using DigiStore.TgBot.Application.Handlers;
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
	public const string CallbackData = DigiStore.TgBot.Application.Constants.CallbackData.BalanceView;
	public const bool IsPrefix = false;
	
	private readonly ITelegramWalletService _walletService;
	private readonly ITelegramSessionService _sessionService;
	private readonly ILogger<BalanceView> _logger;

	string ICallbackQueryHandler.CallbackData => CallbackData;
	bool ICallbackQueryHandler.IsPrefix => IsPrefix;

	public BalanceView(
		ITelegramBotClient botClient,
		ITelegramWalletService walletService,
		ITelegramSessionService sessionService,
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
			var session = await _sessionService.GetSessionAsync(telegramId, cancellationToken);

			if (session?.UserId == null)
				return;

			var languageCode = session.LanguageCode ?? "en";

			var walletResult = await _walletService.GetBalanceAsync(session.UserId, cancellationToken);

			if (!walletResult.IsSuccess)
			{
				await AnswerCallbackQueryWithError(callbackQuery.Id, languageCode, cancellationToken);
				return;
			}

			var wallet = walletResult.Value!;
			var text = $@"
💰 {_localizationService.GetMessage("balance_info", languageCode)}

{_localizationService.GetMessage("current_balance", languageCode)}: <b>{wallet.Balance:F2} {wallet.Currency}</b>
📊 {_localizationService.GetMessage("total_deposited", languageCode)}: {wallet.TotalDeposited:F2} {wallet.Currency}
📤 {_localizationService.GetMessage("total_withdrawn", languageCode)}: {wallet.TotalWithdrawn:F2} {wallet.Currency}
";

			var keyboard = GetBackToMainMenuKeyboard(languageCode);

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
			await AnswerCallbackQueryWithError(callbackQuery.Id, "en", cancellationToken);
		}
	}
}
