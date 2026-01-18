using CSharpFunctionalExtensions;
using DigiStore.Enums;
using DigiStore.SharedKernel;
using DigiStore.SharedKernel.Extensions;
using DigiStore.TgBot.Application.Constants;
using DigiStore.TgBot.Application.Handlers.Adstracts;
using DigiStore.TgBot.Application.Interfaces.Services;
using DigiStore.UserService.Contracts.Enums;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace DigiStore.TgBot.Application.Handlers.Callbacks;

/// <summary>
/// Обработчик колбэка главного меню
/// </summary>
public class TopUpBalance : BaseHandler, ICallbackQueryHandler
{
	//public const string CallbackData = Constants.CallbackData.BalanceTopPrefix;
	public const string CallbackData = "topup_do_";
	public const bool IsPrefix = true;
	
	private readonly ISessionService _sessionService;
    private readonly IWalletService _walletService;
    private readonly ILogger<MainMenu> _logger;


	public TopUpBalance(
		ITelegramBotClient botClient,
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

		var languageCode = LanguageCodes.en;
		if (sessionResult.IsSuccess)
			languageCode = sessionResult.Value.LangCode;

	var payAggregatResult = callbackQuery.Data.Replace(CallbackData, "").ParseEnum<PaymentAggregators>();
		if (payAggregatResult.IsFailure)
		{
			await AnswerCallbackQueryWithError(callbackQuery.Id, LanguageCodes.en, token);
			return payAggregatResult.Error;
		}
		var payAggregate = payAggregatResult.Value;

		_logger.LogInformation("Top up balance from agregattor: {Aggregate}, UserId: {UserId}", payAggregate, session.UserId);


		var amount = session.PendingTopUpAmount ?? 15;
		var result = await _walletService.CreatePaymentAsync(session.UserId, payAggregate, amount, token);
		if(result.IsFailure)
		{
			await AnswerCallbackQueryWithError(callbackQuery.Id, LanguageCodes.en, token);
			return result.Error;
		}

		// Update session
		session.SetState(BotState.TopUpBalance);
		session.PendingTopUpAmount = null;
		session.PendingTopUpAggregator = null;
		await _sessionService.UpdateSessionAsync(session, token);


		var text =	$"Для пополнения баланса на {""}р. перейди по ссылке:\n" +
					$"{result.Value}\n\n" +
					$"После оплаты баланс профиля пополнится автоматически.";

		var keyboard = new InlineKeyboardMarkup(new[]
		{
			new[]
			{
				InlineKeyboardButton.WithUrl("Оплатить", result.Value)
			},

			new[]
			{
				InlineKeyboardButton.WithCallbackData(_localService.GetMessage(LocalKeys.Buttons.Back, languageCode), Constants.CallbackData.BalanceView)
			},
		});



		try
		{
			await _botClient.EditMessageText(
				callbackQuery.Message.Chat.Id,
				894,
				text,
				parseMode: ParseMode.Html,
				replyMarkup: keyboard,
				cancellationToken: token);

			_logger.LogInformation("Top up balance for user: {UserId}", session.UserId);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error in LanguageSelectionCallbackHandler");
			await AnswerCallbackQueryWithError(callbackQuery.Id, LanguageCodes.en, token);
			return Error.Failure("callback.langselect.error", "Error in LanguageSelectionCallbackHandler");
		}

		return Result.Success<Error>();
	}
}
