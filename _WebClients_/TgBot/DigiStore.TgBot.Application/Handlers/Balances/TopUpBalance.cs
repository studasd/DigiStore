using CSharpFunctionalExtensions;
using DigiStore.Enums;
using DigiStore.TgBot.Application.Constants;
using DigiStore.TgBot.Application.Handlers.Adstracts;
using DigiStore.TgBot.Application.Handlers.Menu;
using DigiStore.TgBot.Application.Interfaces.Services;
using Microsoft.Extensions.Logging;
using StudCoreKit.SharedKernel;
using StudCoreKit.SharedKernel.Extensions;
using StudTgBotApi.Contracts.Interfaces;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace DigiStore.TgBot.Application.Handlers.Balances;

/// <summary>
/// Обработчик колбэка главного меню
/// </summary>
public class TopUpBalance : BaseHandler, ICallbackHandler
{
	//public const string CallbackData = Constants.CallbackData.BalanceTopPrefix;
	public const string CallbackData = Constants.CallbackData.BalanceUpPrefix;
	public const bool IsPrefix = true;
	
	private readonly ISessionService _sessionService;
    private readonly IWalletService _walletService;
    private readonly ILogger<MainMenu> _logger;


	public TopUpBalance(
		IBotAPIClient botClient,
		ISessionService sessionService,
		ILocalizationService localizationService,
		IWalletService walletService,
		ILogger<MainMenu> logger)
		: base(botClient, localizationService)
	{
		_sessionService = sessionService;
        _walletService = walletService;
        _logger = logger;
	}

	public async Task<UnitResult<Error>> HandleAsync(CallbackQuery callbackQuery, CancellationToken token = default)
	{
		// Handle language selection from /start command

		if (callbackQuery.Data == null || callbackQuery.Message == null)
			return Error.Failure("callback.topupbalance.nodata", "No data in TopUpBalanceCallbackHandler");


		var telegramId = callbackQuery.From.Id;
		var sessionResult = await _sessionService.GetSessionAsync(telegramId, token);

		if (sessionResult.IsFailure)
			return sessionResult.Error;

		var session = sessionResult.Value;

		var langCode = LanguageCodes.en;
		if (sessionResult.IsSuccess)
			langCode = sessionResult.Value.LangCode;

		// Data can be either: "topup_do_{Aggregator}" or "topup_do_{Aggregator}:{chatId}:{messageId}"
		var raw = callbackQuery.Data.Replace(CallbackData, "");
		var parts = raw.Split(':', StringSplitOptions.RemoveEmptyEntries);
		var aggPart = parts.Length > 0 ? parts[0] : raw;
		var payAggregatResult = aggPart.ParseEnum<PaymentAggregators>();
		if (payAggregatResult.IsFailure)
		{
			await AnswerCallbackQueryWithError(callbackQuery.Id, LanguageCodes.en, token);
			return payAggregatResult.Error;
		}
		var payAggregate = payAggregatResult.Value;

		_logger.LogInformation("Top up balance from agregattor: {Aggregate}, UserId: {UserId}", payAggregate, session.UserId);


		var targetChatId = callbackQuery.Message.Chat.Id;
		var targetMessageId = callbackQuery.Message.MessageId;
		if (parts.Length == 3 && long.TryParse(parts[1], out var parsedChatId) && int.TryParse(parts[2], out var parsedMessageId))
		{
			targetChatId = parsedChatId;
			targetMessageId = parsedMessageId;
		}

		var ctx = session.GetMessageContext(targetChatId, targetMessageId);
		var amount = ctx?.PendingTopUp?.Amount ?? 100;
		var result = await _walletService.CreatePaymentAsync(session.UserId, payAggregate, amount, token);
		if(result.IsFailure)
		{
			await AnswerCallbackQueryWithError(callbackQuery.Id, LanguageCodes.en, token);
			return result.Error;
		}

		var paymentId = result.Value.paymentId;

		// Update session
		session.SetState(BotState.TopUpBalance);
		var editChatId = targetChatId;
		var editMessageId = targetMessageId;
		// PendingTopUpChatId/PendingTopUpMessageId are intentionally preserved.
		// They are required later to edit the payment message after successful webhook completion.
		session.RemoveMessageContext(editChatId, editMessageId);
		session.PendingPayments[paymentId] = new Domain.ValueObjects.PendingPaymentMessageVO(
			editChatId,
			editMessageId,
			amount,
			payAggregate.ToString());
		await _sessionService.UpdateSessionAsync(session, token);


		var model = new
		{
			amount = amount,
			url = result.Value.redirectUrl,
		};

		var text = _localService.GetMessage(LocalKeys.Templates.TopUpBalance, langCode, model);

		var keyboard = new InlineKeyboardMarkup(new[]
		{
			new[]
			{
				InlineKeyboardButton.WithUrl(_localService.GetMessage(LocalKeys.Buttons.Pay, langCode), result.Value.redirectUrl)
			},

			new[]
			{
				InlineKeyboardButton.WithCallbackData(_localService.GetMessage(LocalKeys.Buttons.Back, langCode), Constants.CallbackData.BalanceCallback)
			},
		});


		var editResult = await _botClient.EditMessageTextAsync(
			editChatId,
			editMessageId,
			text,
			parseMode: ParseMode.Html,
			replyMarkup: keyboard,
			cancellationToken: token);

		if (editResult.IsFailure)
		{
			_logger.LogError("Failed to edit message in TopUpBalanceCallbackHandler for user: {UserId}", session.UserId);
			return await AnswerCallbackQueryWithError(callbackQuery.Id, LanguageCodes.en, token);
		}

		_logger.LogInformation("Top up balance for user: {UserId}", session.UserId);

		return await _botClient.AnswerCallbackQueryAsync(callbackQuery.Id, cancellationToken: token);
	}
}
