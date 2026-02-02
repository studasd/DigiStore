using CSharpFunctionalExtensions;
using DigiStore.TgBot.Application.Constants;
using DigiStore.TgBot.Application.Handlers.Adstracts;
using DigiStore.TgBot.Application.Interfaces.Services;
using Microsoft.Extensions.Logging;
using StudCoreKit.SharedKernel;
using StudTgBotApi.Contracts.Interfaces;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace DigiStore.TgBot.Application.Handlers.Balances;

/// <summary>
/// Обработка ввода суммы пополнения после выбора агрегатора.
/// </summary>
public class TopUpAmountInput : BaseHandler, IInputMessageHandler
{
	public string StateKey => BotState.TopUpBalanceAmountAwaiting;
	//public string State => StateKey;
	//public const string Command = BotCommands.Balance;


	private readonly ISessionService _sessionService;
	private readonly TopUpBalance _topUpBalance;
	private readonly ILogger<TopUpAmountInput> _logger;

	public TopUpAmountInput(
		IBotAPIClient botClient,
		ISessionService sessionService,
		ILocalizationService localizationService,
		TopUpBalance topUpBalance,
		ILogger<TopUpAmountInput> logger) : base(botClient, localizationService)
	{
		_sessionService = sessionService;
		_topUpBalance = topUpBalance;
		_logger = logger;
	}

	public async Task<UnitResult<Error>> HandleAsync(Message message, CancellationToken token = default)
	{
		if (message.From == null)
			return Error.Failure("input.topupamount.nofrom", "No From in message");

		var sessionResult = await _sessionService.GetSessionAsync(message.From.Id, token);
		if (sessionResult.IsFailure)
			return sessionResult.Error;

		var session = sessionResult.Value;
		int? userMessageIdToDelete = message.MessageId;

		// Find active message-context for amount input (latest updated context in awaiting state)
		var ctxEntry = session.MessageContexts
			.Where(x => x.Value.State == BotState.TopUpBalanceAmountAwaiting)
			.OrderByDescending(x => x.Value.UpdatedAtUtc)
			.FirstOrDefault();

		var pending = ctxEntry.Value?.PendingTopUp;
		var editChatId = pending?.ChatId ?? message.Chat.Id;
		var editMessageId = pending?.MessageId;

		var raw = (message.Text ?? string.Empty).Trim().Replace(',', '.');

		var langCode = session.LangCode;
		var keyboard = new InlineKeyboardMarkup(new[]
		{
			new[]
			{
				InlineKeyboardButton.WithCallbackData(_localService.GetMessage(LocalKeys.Buttons.Back, langCode), CallbackData.BalanceView)
			},
		});


		if (!decimal.TryParse(raw, System.Globalization.CultureInfo.InvariantCulture, out var amount) || amount <= 0)
		{
			if (userMessageIdToDelete.HasValue)
			{
				await _botClient.DeleteMessageAsync(message.Chat.Id, userMessageIdToDelete.Value, cancellationToken: token);
			}
			
			if (editMessageId.HasValue)
			{
				var editResult = await _botClient.EditMessageTextAsync(
					editChatId,
					editMessageId.Value,
					_localService.GetMessage(LocalKeys.Messages.TopUpAmountInputErrorAmount, langCode),
					replyMarkup: keyboard,
					cancellationToken: token);

				if (editResult.IsFailure)
				{
					_logger.LogError("Error in MainMenuCallbackHandler");
					return editResult.Error;
				}
			}
			return Result.Success<Error>();
		}

		if (string.IsNullOrWhiteSpace(pending?.Aggregator) || !Enum.TryParse<Enums.PaymentAggregators>(pending?.Aggregator, out var aggregator))
		{
			if (userMessageIdToDelete.HasValue)
			{
				await _botClient.DeleteMessageAsync(message.Chat.Id, userMessageIdToDelete.Value, cancellationToken: token);
			}

			if (editMessageId.HasValue)
			{
				var editResult = await _botClient.EditMessageTextAsync(
					editChatId,
					editMessageId.Value,
					_localService.GetMessage(LocalKeys.Messages.TopUpAmountInputErrorAggregator, langCode),
					replyMarkup: keyboard,
					cancellationToken: token);

				if (editResult.IsFailure)
				{
					_logger.LogError("Error in MainMenuCallbackHandler");
				}
			}

			session.SetState(BotState.BalanceViewing);
			if (!string.IsNullOrWhiteSpace(ctxEntry.Key) && pending?.ChatId is not null && pending?.MessageId is not null)
			{
				session.RemoveMessageContext(pending.ChatId.Value, pending.MessageId.Value);
			}

			var updateResult = await _sessionService.UpdateSessionAsync(session, token);
			if (updateResult.IsFailure)
				return updateResult.Error;

			return Result.Success<Error>();
		}

		if (pending?.ChatId is null || pending?.MessageId is null)
		{
			return Result.Success<Error>();
		}

		session.UpsertMessageContext(
			pending.ChatId.Value,
			pending.MessageId.Value,
			new Domain.ValueObjects.MessageContextVO(
				BotState.TopUpBalance,
				pending with { Amount = amount },
				DateTime.UtcNow));

		session.SetState(BotState.TopUpBalance);
		await _sessionService.UpdateSessionAsync(session, token);

		if (userMessageIdToDelete.HasValue)
		{
			await _botClient.DeleteMessageAsync(message.Chat.Id, userMessageIdToDelete.Value, cancellationToken: token);
		}

		var cb = new CallbackQuery
		{
			Id = Guid.NewGuid().ToString("N"),
			From = message.From,
			Message = message,
			Data = TopUpBalance.CallbackData + aggregator + $":{pending.ChatId.Value}:{pending.MessageId.Value}"
		};

		var nextResult = await _topUpBalance.HandleAsync(cb, token);
		if (nextResult.IsFailure)
			return nextResult.Error;

		return Result.Success<Error>();
	}
}
