using CSharpFunctionalExtensions;
using StudCoreKit.Framework.Endpoints;
using StudCoreKit.SharedKernel;
using DigiStore.TgBot.Application.Interfaces;
using DigiStore.TgBot.Application.Interfaces.Repositories;
using DigiStore.TgBot.Application.Interfaces.Services;
using DigiStore.TgBot.Contracts.Requests;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using StudTgBotApi.Contracts.Interfaces;

namespace DigiStore.TgBot.Application.Features;

/// успешный платеж пополния баланса
public sealed class UpdatePayment : IEndpoint
{
	//[Authorize]
	public void MapEndpoint(IEndpointRouteBuilder app)
	{
		app.MapPost("updatePayment/{userId}", async Task<EndpointResult> (
			[FromRoute] Guid userId,
			[FromBody] UpdatePaymentRequest request,
			[FromServices] UpdatePaymentHandler handler,
			CancellationToken token) =>
		{
			return await handler.Handle(userId, request, token);
		});
	}
}



public sealed class UpdatePaymentHandler : ITgBotHandler
{
	private readonly IBotAPIClient _botClient;
	private readonly ISessionService _sessionService;
	private readonly ITgUserRepository _tgUserRepository;
	private readonly IProfileService _profileService;
	private readonly ILogger<UpdatePaymentHandler> _logger;

	public UpdatePaymentHandler(
		IBotAPIClient botClient,
		ISessionService sessionService,
		ITgUserRepository tgUserRepository,
		IProfileService profileService,
		ILogger<UpdatePaymentHandler> logger)
	{
		_botClient = botClient;
		_sessionService = sessionService;
		_tgUserRepository = tgUserRepository;
		_profileService = profileService;
		_logger = logger;
	}

	public async Task<UnitResult<Error>> Handle(Guid userId, UpdatePaymentRequest request, CancellationToken token)
	{
		// 1) Map userId -> telegramId
		var tgUserResult = await _tgUserRepository.GetByUserIdAsync(userId, token);
		if (tgUserResult.IsFailure)
		{
			_logger.LogWarning("UpdatePayment: TgUser not found by UserId={UserId}: {Error}", userId, tgUserResult.Error.GetMessage());
			return tgUserResult.Error;
		}

		var telegramId = tgUserResult.Value.TelegramId;

		// 2) Load session to locate the message with payment link
		var sessionResult = await _sessionService.GetSessionAsync(telegramId, token);
		if (sessionResult.IsFailure)
		{
			_logger.LogWarning("UpdatePayment: session not found for TelegramId={TelegramId}: {Error}", telegramId, sessionResult.Error.GetMessage());
			return sessionResult.Error;
		}

		var session = sessionResult.Value;

		long? chatId = null;
		int? messageId = null;
		decimal? amount = null;

		if (request.PaymentId != Guid.Empty && session.PendingPayments.TryGetValue(request.PaymentId, out var pending))
		{
			chatId = pending.ChatId;
			messageId = pending.MessageId;
			amount = pending.Amount;
		}
		// No legacy fallback: payment completion must be correlated by PaymentId.

		// If message identifiers are missing - idempotent success (nothing to update)
		if (!chatId.HasValue || !messageId.HasValue)
		{
			_logger.LogInformation("UpdatePayment: no pending top up message for UserId={UserId} TelegramId={TelegramId}", userId, telegramId);
			return Result.Success<Error>();
		}

		// 3) Update the original message containing payment URL
		var successText = amount.HasValue
			? $"✅ Платёж на <b>{amount.Value}</b> успешно зачислен. Баланс обновлён."
			: "✅ Платёж успешно зачислен. Баланс обновлён.";
		var editResult = await _botClient.EditMessageTextAsync(
			new ChatId(chatId.Value),
			messageId.Value,
			successText,
			parseMode: ParseMode.Html,
			replyMarkup: null,
			cancellationToken: token);

		if (editResult.IsFailure)
		{
			_logger.LogWarning("UpdatePayment: failed to edit payment message ChatId={ChatId} MessageId={MessageId}: {Error}", chatId.Value, messageId.Value, editResult.Error.GetMessage());
			return editResult.Error;
		}

		// 4) Send updated profile (with new balance)
		var profileResult = await _profileService.GetFullProfileAsync(userId, telegramId, token);
		if (profileResult.IsFailure)
		{
			_logger.LogWarning("UpdatePayment: failed to get profile for UserId={UserId}: {Error}", userId, profileResult.Error.GetMessage());
			return profileResult.Error;
		}

		var (profileText, keyboard) = _profileService.BuildProfileMessage(profileResult.Value, session.LangCode);
		var sendProfileResult = await _botClient.SendMessageAsync(
			new ChatId(chatId.Value),
			profileText,
			parseMode: ParseMode.Html,
			replyMarkup: keyboard,
			cancellationToken: token);

		if (sendProfileResult.IsFailure)
		{
			_logger.LogWarning("UpdatePayment: failed to send profile ChatId={ChatId}: {Error}", chatId.Value, sendProfileResult.Error.GetMessage());
			return sendProfileResult.Error;
		}

		// 5) Clear pending fields to prevent re-editing
		if (request.PaymentId != Guid.Empty)
		{
			session.PendingPayments.Remove(request.PaymentId);
		}

		// Payment completion is keyed by PaymentId; per-message contexts are not required here.
		await _sessionService.UpdateSessionAsync(session, token);

		_logger.LogInformation("UpdatePayment: handled successfully for UserId={UserId} TelegramId={TelegramId}", userId, telegramId);
		return Result.Success<Error>();
	}
}