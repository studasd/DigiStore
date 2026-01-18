using CSharpFunctionalExtensions;
using DigiStore.SharedKernel;
using DigiStore.TgBot.Application.Constants;
using DigiStore.TgBot.Application.Handlers.Adstracts;
using DigiStore.TgBot.Application.Handlers.Callbacks;
using DigiStore.TgBot.Application.Interfaces.Services;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace DigiStore.TgBot.Application.Handlers.InputMessages;

/// <summary>
/// Обработка ввода суммы пополнения после выбора агрегатора.
/// </summary>
public class TopUpAmountInput : BaseHandler, IInputMessageHandler
{
	public const string StateKey = BotState.TopUpBalanceAmountAwaiting;
	//public string State => StateKey;
	//public const string Command = BotCommands.Balance;


	private readonly ISessionService _sessionService;
	private readonly TopUpBalance _topUpBalance;
	private readonly ILogger<TopUpAmountInput> _logger;

	public TopUpAmountInput(
		ITelegramBotClient botClient,
		ISessionService sessionService,
		ILocalizationService localizationService,
		DigiStore.TgBot.Application.Handlers.Callbacks.TopUpBalance topUpBalance,
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

		Message? originalBotMessage = message.ReplyToMessage;

		var raw = (message.Text ?? string.Empty).Trim().Replace(',', '.');
		if (!decimal.TryParse(raw, System.Globalization.CultureInfo.InvariantCulture, out var amount) || amount <= 0)
		{
			if (userMessageIdToDelete.HasValue)
			{
				try
				{
					await _botClient.DeleteMessage(message.Chat.Id, userMessageIdToDelete.Value, cancellationToken: token);
				}
				catch { }
			}
			await _botClient.SendMessage(message.Chat.Id, "Введите корректную сумму числом (больше 0).", cancellationToken: token);
			return Result.Success<Error>();
		}

		if (string.IsNullOrWhiteSpace(session.PendingTopUpAggregator) || !Enum.TryParse<DigiStore.Enums.PaymentAggregators>(session.PendingTopUpAggregator, out var aggregator))
		{
			if (userMessageIdToDelete.HasValue)
			{
				try
				{
					await _botClient.DeleteMessage(message.Chat.Id, userMessageIdToDelete.Value, cancellationToken: token);
				}
				catch { }
			}

			await _botClient.SendMessage(message.Chat.Id, "Не удалось определить способ оплаты. Откройте баланс и выберите агрегатор заново.", cancellationToken: token);
			session.SetState(BotState.BalanceViewing);
			session.PendingTopUpAmount = null;
			session.PendingTopUpAggregator = null;
			await _sessionService.UpdateSessionAsync(session, token);
			return Result.Success<Error>();
		}

		session.PendingTopUpAmount = amount;
		session.SetState(BotState.TopUpBalance);
		await _sessionService.UpdateSessionAsync(session, token);

		if (userMessageIdToDelete.HasValue)
		{
			try
			{
				await _botClient.DeleteMessage(message.Chat.Id, userMessageIdToDelete.Value, cancellationToken: token);
			}
			catch { }
		}

		var cb = new CallbackQuery
		{
			Id = Guid.NewGuid().ToString("N"),
			From = message.From,
			Message = originalBotMessage ?? message,
			Data = TopUpBalance.CallbackData + aggregator
		};

		var nextResult = await _topUpBalance.HandleAsync(cb, token);
		if (nextResult.IsFailure)
			return nextResult.Error;

		return Result.Success<Error>();
	}
}
