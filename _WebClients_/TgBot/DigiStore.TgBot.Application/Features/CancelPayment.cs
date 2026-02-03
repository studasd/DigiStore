using CSharpFunctionalExtensions;
using DigiStore.TgBot.Application.Interfaces;
using DigiStore.TgBot.Application.Interfaces.Repositories;
using DigiStore.TgBot.Application.Interfaces.Services;
using DigiStore.TgBot.Contracts.Requests;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using StudCoreKit.Framework.Endpoints;
using StudCoreKit.SharedKernel;
using StudTgBotApi.Contracts.Interfaces;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace DigiStore.TgBot.Application.Features;

/// <summary>
/// Уведомление TgBot об отмене платежа (платежный агрегатор)
/// </summary>
public sealed class CancelPayment : IEndpoint
{
	public void MapEndpoint(IEndpointRouteBuilder app)
	{
		app.MapPost("cancelPayment/{userId}", async Task<EndpointResult>(
			[FromRoute] Guid userId,
			[FromBody] CancelPaymentRequest request,
			[FromServices] CancelPaymentHandler handler,
			CancellationToken token) =>
		{
			return await handler.Handle(userId, request, token);
		});
	}
}

public sealed class CancelPaymentHandler : ITgBotHandler
{
	private readonly IBotAPIClient _botClient;
	private readonly ISessionService _sessionService;
	private readonly ITgUserRepository _tgUserRepository;
	private readonly ILogger<CancelPaymentHandler> _logger;

	public CancelPaymentHandler(
		IBotAPIClient botClient,
		ISessionService sessionService,
		ITgUserRepository tgUserRepository,
		ILogger<CancelPaymentHandler> logger)
	{
		_botClient = botClient;
		_sessionService = sessionService;
		_tgUserRepository = tgUserRepository;
		_logger = logger;
	}

	public async Task<UnitResult<Error>> Handle(Guid userId, CancelPaymentRequest request, CancellationToken token)
	{
		var tgUserResult = await _tgUserRepository.GetByUserIdAsync(userId, token);
		if (tgUserResult.IsFailure)
		{
			_logger.LogWarning("CancelPayment: TgUser not found by UserId={UserId}: {Error}", userId, tgUserResult.Error.GetMessage());
			return tgUserResult.Error;
		}

		var telegramId = tgUserResult.Value.TelegramId;

		var sessionResult = await _sessionService.GetSessionAsync(telegramId, token);
		if (sessionResult.IsFailure)
		{
			_logger.LogWarning("CancelPayment: session not found for TelegramId={TelegramId}: {Error}", telegramId, sessionResult.Error.GetMessage());
			return sessionResult.Error;
		}

		var session = sessionResult.Value;

		if (request.PaymentId == Guid.Empty || !session.PendingPayments.TryGetValue(request.PaymentId, out var pending))
		{
			_logger.LogInformation("CancelPayment: no pending payment mapping for UserId={UserId} TelegramId={TelegramId} PaymentId={PaymentId}", userId, telegramId, request.PaymentId);
			return Result.Success<Error>();
		}


		// Устанавливаем контекст бота на основании сохраненного BotId
		var botId = pending.BotId;
		_botClient.SetContext(botId);

		var cancelText = string.IsNullOrWhiteSpace(request.Reason)
			? "? Платёж отменён. Списание не выполнено."
			: $"? Платёж отменён. Причина: \n<b>{request.Reason}</b>";

		var editResult = await _botClient.EditMessageTextAsync(
			new ChatId(307723779),
			1082,
			cancelText,
			parseMode: ParseMode.Html,
			replyMarkup: null,
			cancellationToken: token);

		if (editResult.IsFailure)
		{
			_logger.LogWarning("CancelPayment: failed to edit payment message ChatId={ChatId} MessageId={MessageId}: {Error}", pending.ChatId, pending.MessageId, editResult.Error.GetMessage());
			return editResult.Error;
		}

		session.PendingPayments.Remove(request.PaymentId);
		await _sessionService.UpdateSessionAsync(session, token);

		_logger.LogInformation("CancelPayment: handled successfully for UserId={UserId} TelegramId={TelegramId} PaymentId={PaymentId}", userId, telegramId, request.PaymentId);
		return Result.Success<Error>();
	}
}
