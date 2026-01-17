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

namespace DigiStore.TgBot.Application.Handlers.Callbacks;

/// <summary>
/// Обработчик колбэка главного меню
/// </summary>
public class TopUpBalance : BaseHandler, ICallbackQueryHandler
{
	public const string CallbackData = Constants.CallbackData.BalanceTopPrefix;
	public const bool IsPrefix = true;
	
	private readonly ISessionService _sessionService;
	private readonly ILogger<MainMenu> _logger;


	public TopUpBalance(
		ITelegramBotClient botClient,
		ISessionService sessionService,
		ILocalizationService localizationService,
		ILogger<MainMenu> logger)
		: base(botClient, localizationService)
	{
		_sessionService = sessionService;
		_logger = logger;
	}

	public async Task<UnitResult<Error>> HandleAsync(CallbackQuery callbackQuery, CancellationToken cancellationToken = default)
	{
		// Handle language selection from /start command

		if (callbackQuery.Data == null || callbackQuery.Message == null)
			return Error.Failure("callback.topupbalance.nodata", "No data in TopUpBalanceCallbackHandler");


		var telegramId = callbackQuery.From.Id;
		var sessionResult = await _sessionService.GetSessionAsync(telegramId, cancellationToken);

		if (sessionResult.IsFailure)
			return sessionResult.Error;

		var session = sessionResult.Value;

		var payAggregatResult = callbackQuery.Data.Replace(CallbackData, "").ParseEnum<PaymentAggregators>();
		if (payAggregatResult.IsFailure)
		{
			await AnswerCallbackQueryWithError(callbackQuery.Id, LanguageCodes.en, cancellationToken);
			return payAggregatResult.Error;
		}
		var payAggregate = payAggregatResult.Value;

		_logger.LogInformation("Language selected from /start: {LanguageCode}, UserId: {UserId}", payAggregate, session.UserId);

		//// Update user language in UserService
		//var updateResult = await _profileService.UpdateUserLanguageAsync(
		//	session.UserId,
		//	payAggregate,
		//	cancellationToken);

		//if (updateResult.IsFailure)
		//{
		//	await AnswerCallbackQueryWithError(callbackQuery.Id, payAggregate, cancellationToken);
		//	return updateResult.Error;
		//}

		//// Update session
		//session.LangCode = payAggregate;
		//session.SetState(BotState.LanguageSelected);
		//await _sessionService.UpdateSessionAsync(session, cancellationToken);

		


		try
		{
			//await _botClient.EditMessageText(
			//	callbackQuery.Message.Chat.Id,
			//	callbackQuery.Message.MessageId,
			//	profileText,
			//	parseMode: ParseMode.Html,
			//	replyMarkup: keyboard,
			//	cancellationToken: cancellationToken);

			await _botClient.AnswerCallbackQuery(callbackQuery.Id, cancellationToken: cancellationToken);

			_logger.LogInformation(
				"Profile shown after language selection for user: {UserId}",
				session.UserId);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error in LanguageSelectionCallbackHandler");
			await AnswerCallbackQueryWithError(callbackQuery.Id, LanguageCodes.en, cancellationToken);
			return Error.Failure("callback.langselect.error", "Error in LanguageSelectionCallbackHandler");
		}

		return Result.Success<Error>();
	}
}
