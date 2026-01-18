using CSharpFunctionalExtensions;
using DigiStore.Enums;
using DigiStore.SharedKernel;
using DigiStore.SharedKernel.Extensions;
using DigiStore.TgBot.Application.Constants;
using DigiStore.TgBot.Application.Handlers.Adstracts;
using DigiStore.TgBot.Application.Interfaces.Services;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace DigiStore.TgBot.Application.Handlers.Callbacks;

/// <summary>
/// Промежуточный обработчик: после выбора агрегатора запрашивает сумму пополнения.
/// </summary>
public class TopUpAmountRequest : BaseHandler, ICallbackQueryHandler
{
	public const string CallbackData = Constants.CallbackData.BalanceTopPrefix;
	public const bool IsPrefix = true;

	private readonly ISessionService _sessionService;
	private readonly ILogger<TopUpAmountRequest> _logger;

	public TopUpAmountRequest(
		ITelegramBotClient botClient,
		ISessionService sessionService,
		ILocalizationService localizationService,
		ILogger<TopUpAmountRequest> logger)
		: base(botClient, localizationService)
	{
		_sessionService = sessionService;
		_logger = logger;
	}

	public async Task<UnitResult<Error>> HandleAsync(CallbackQuery callbackQuery, CancellationToken token = default)
	{
		if (callbackQuery.Data == null || callbackQuery.Message == null)
			return Error.Failure("callback.topupamount.nodata", "No data/message in TopUpAmountRequest");

		var telegramId = callbackQuery.From.Id;
		var sessionResult = await _sessionService.GetSessionAsync(telegramId, token);
		if (sessionResult.IsFailure)
		{
			await AnswerCallbackQueryWithError(callbackQuery.Id, LanguageCodes.en, token);
			return sessionResult.Error;
		}

		var session = sessionResult.Value;
		var langCode = session.LangCode;

		var payAggregatResult = callbackQuery.Data.Replace(CallbackData, "").ParseEnum<PaymentAggregators>();
		if (payAggregatResult.IsFailure)
		{
			await AnswerCallbackQueryWithError(callbackQuery.Id, langCode, token);
			return payAggregatResult.Error;
		}

		session.PendingTopUpAggregator = payAggregatResult.Value.ToString();
		session.PendingTopUpAmount = null;
		session.PendingTopUpChatId = callbackQuery.Message.Chat.Id;
		session.PendingTopUpMessageId = callbackQuery.Message.MessageId;
		session.SetState(BotState.TopUpBalanceAmountAwaiting);
		await _sessionService.UpdateSessionAsync(session, token);

		var text = "Введите сумму пополнения числом (например 1500).";
		var keyboard = new InlineKeyboardMarkup(new[]
		{
			new[]
			{
				InlineKeyboardButton.WithCallbackData(_localService.GetMessage(LocalKeys.Buttons.Back, langCode), Constants.CallbackData.BalanceView)
			},
		});

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
			_logger.LogError(ex, "Error in TopUpAmountRequest");
			await AnswerCallbackQueryWithError(callbackQuery.Id, langCode, token);
			return Error.Failure("callback.topupamount.error", "Error in TopUpAmountRequest");
		}

		return Result.Success<Error>();
	}
}
