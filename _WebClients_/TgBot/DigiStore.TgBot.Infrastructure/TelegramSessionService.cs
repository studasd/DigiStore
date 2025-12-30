using DigiStore.TgBot.Application.Constants;
using DigiStore.TgBot.Application.Interfaces;
using DigiStore.TgBot.Domain;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace DigiStore.TgBot.Infrastructure;


public class TelegramSessionService : ITelegramSessionService
{
	private readonly IDatabase _db;
	private readonly ILogger<TelegramSessionService> _logger;
	private const string SessionKeyFormat = "tg:session:{0}";
	private readonly TimeSpan _sessionExpiration = TimeSpan.FromHours(24);

	public TelegramSessionService(
		ILogger<TelegramSessionService> logger)
	{
		_logger = logger;
	}

	public async Task<TelegramUserSession> GetOrCreateSessionAsync(long telegramId, CancellationToken ct = default)
	{
		var key = string.Format(SessionKeyFormat, telegramId);
		var value = await _db.StringGetAsync(key);

		if (value.HasValue)
		{
			var session = JsonSerializer.Deserialize<TelegramUserSession>(value.ToString());
			_logger.LogDebug("Session retrieved from cache for Telegram ID: {TelegramId}", telegramId);
			return session!;
		}

		var newSession = new TelegramUserSession
		{
			TelegramId = telegramId,
			CurrentState = BotState.Start,
			CreatedAt = DateTime.UtcNow
		};

		await SaveSessionAsync(newSession, ct);
		_logger.LogInformation("New session created for Telegram ID: {TelegramId}", telegramId);

		return newSession;
	}


	public async Task UpdateSessionAsync(TelegramUserSession session, CancellationToken ct = default)
	{
		session.UpdateActivity();
		await SaveSessionAsync(session, ct);
	}


	public async Task ClearSessionAsync(long telegramId, CancellationToken ct = default)
	{
		var key = string.Format(SessionKeyFormat, telegramId);
		await _db.KeyDeleteAsync(key);
		_logger.LogInformation("Session cleared for Telegram ID: {TelegramId}", telegramId);
	}


	public async Task<TelegramUserSession?> GetSessionAsync(long telegramId, CancellationToken ct = default)
	{
		var key = string.Format(SessionKeyFormat, telegramId);
		var value = await _db.StringGetAsync(key);

		if (!value.HasValue)
			return null;

		return JsonSerializer.Deserialize<TelegramUserSession>(value.ToString());
	}


	private async Task SaveSessionAsync(TelegramUserSession session, CancellationToken ct)
	{
		var key = string.Format(SessionKeyFormat, session.TelegramId);
		var json = JsonSerializer.Serialize(session);
		await _db.StringSetAsync(key, json, _sessionExpiration);
	}
}
