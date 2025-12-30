using DigiStore.TgBot.Application.Constants;
using DigiStore.TgBot.Application.Handlers.Attributes;
using DigiStore.TgBot.Application.Interfaces;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace DigiStore.TgBot.Application.Handlers.Callbacks;

/// <summary>
/// Обработчик колбэка просмотра баланса
/// </summary>
[CallbackQuery(CallbackData.BalanceView)]
public class BalanceViewCallbackHandler : ICallbackQueryHandler
{
	private readonly ITelegramWalletService _walletService;
	private readonly ITelegramSessionService _sessionService;
	private readonly ILocalizationService _localizationService;
	private readonly ILogger<BalanceViewCallbackHandler> _logger;

	public BalanceViewCallbackHandler(
		ITelegramWalletService walletService,
		ITelegramSessionService sessionService,
		ILocalizationService localizationService,
		ILogger<BalanceViewCallbackHandler> logger)
	{
		_walletService = walletService;
		_sessionService = sessionService;
		_localizationService = localizationService;
		_logger = logger;
	}

	public async Task HandleAsync(ITelegramBotClient botClient, CallbackQuery callbackQuery, CancellationToken cancellationToken = default)
	{
		if (callbackQuery.Message == null)
			return;

		try
		{
			var telegramId = callbackQuery.From.Id;
			var session = await _sessionService.GetSessionAsync(telegramId, cancellationToken);

			if (session?.UserId == null)
				return;

			var languageCode = session.LanguageCode ?? "en";
			var loc = _localizationService;

			var walletResult = await _walletService.GetBalanceAsync(session.UserId.Value, cancellationToken);

			if (!walletResult.IsSuccess)
			{
				await botClient.AnswerCallbackQuery(
					callbackQuery.Id,
					loc.GetMessage("error_occurred", languageCode),
					showAlert: true,
					cancellationToken: cancellationToken);
				return;
			}

			var wallet = walletResult.Value!;
			var text = $@"
💰 {loc.GetMessage("balance_info", languageCode)}

{loc.GetMessage("current_balance", languageCode)}: <b>{wallet.Balance:F2} {wallet.Currency}</b>
📊 {loc.GetMessage("total_deposited", languageCode)}: {wallet.TotalDeposited:F2} {wallet.Currency}
📤 {loc.GetMessage("total_withdrawn", languageCode)}: {wallet.TotalWithdrawn:F2} {wallet.Currency}
";

			var keyboard = new InlineKeyboardMarkup(new[]
			{
				new[]
				{
					InlineKeyboardButton.WithCallbackData(
						loc.GetMessage("back", languageCode),
						CallbackData.MenuMain)
				},
			});

			await botClient.EditMessageText(
				callbackQuery.Message.Chat.Id,
				callbackQuery.Message.MessageId,
				text,
				parseMode: ParseMode.Html,
				replyMarkup: keyboard,
				cancellationToken: cancellationToken);

			await botClient.AnswerCallbackQuery(callbackQuery.Id, cancellationToken: cancellationToken);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error in BalanceViewCallbackHandler");
			try
			{
				await botClient.AnswerCallbackQuery(
					callbackQuery.Id,
					_localizationService.GetMessage("error_occurred", "en"),
					showAlert: true,
					cancellationToken: cancellationToken);
			}
			catch { }
		}
	}
}

