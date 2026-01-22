using CSharpFunctionalExtensions;
using DigiStore.Enums;
using DigiStore.SharedKernel;
using DigiStore.SharedKernel.Extensions;
using DigiStore.TgBot.Application.Constants;
using DigiStore.TgBot.Application.Handlers.Adstracts;
using DigiStore.TgBot.Application.Interfaces;
using DigiStore.TgBot.Application.Interfaces.Services;
using Microsoft.Extensions.Logging;
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
		IBotAPIClient botClient,
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

		session.UpsertMessageContext(
			callbackQuery.Message.Chat.Id,
			callbackQuery.Message.MessageId,
			new Domain.ValueObjects.MessageContextVO(
				BotState.TopUpBalanceAmountAwaiting,
				new Domain.ValueObjects.PendingTopUpVO(
					payAggregatResult.Value.ToString(),
					null,
					callbackQuery.Message.Chat.Id,
					callbackQuery.Message.MessageId),
				DateTime.UtcNow));
		session.SetState(BotState.TopUpBalanceAmountAwaiting);
		await _sessionService.UpdateSessionAsync(session, token);

		var text = _localService.GetMessage(LocalKeys.Messages.TopUpAmountRequest, langCode);
		

		var keyboard = new InlineKeyboardMarkup(new[]
		{
			new[]
			{
				InlineKeyboardButton.WithCallbackData(_localService.GetMessage(LocalKeys.Buttons.Back, langCode), Constants.CallbackData.BalanceView)
			},
		});


		var editResult = await _botClient.EditMessageTextAsync(
			callbackQuery.Message.Chat.Id,
			callbackQuery.Message.MessageId,
			text,
			parseMode: ParseMode.Html,
			replyMarkup: keyboard,
			cancellationToken: token);

		if (editResult.IsFailure)
		{
			_logger.LogWarning("Failed to edit message in TopUpAmountRequest: {Reason}", editResult.Error.GetMessage());
			return await AnswerCallbackQueryWithError(callbackQuery.Id, langCode, token);
		}

		return await _botClient.AnswerCallbackQueryAsync(callbackQuery.Id, cancellationToken: token);
	}
}
