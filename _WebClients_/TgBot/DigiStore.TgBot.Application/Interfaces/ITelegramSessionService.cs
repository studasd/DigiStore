using DigiStore.TgBot.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace DigiStore.TgBot.Application.Interfaces;


/// <summary>
/// Service to manage user sessions
/// </summary>
public interface ITelegramSessionService
{
	/// <summary>
	/// Get or create session
	/// </summary>
	Task<TelegramUserSession> GetOrCreateSessionAsync(long telegramId, CancellationToken ct = default);

	/// <summary>
	/// Update session
	/// </summary>
	Task UpdateSessionAsync(TelegramUserSession session, CancellationToken ct = default);

	/// <summary>
	/// Clear session
	/// </summary>
	Task ClearSessionAsync(long telegramId, CancellationToken ct = default);

	/// <summary>
	/// Get session
	/// </summary>
	Task<TelegramUserSession?> GetSessionAsync(long telegramId, CancellationToken ct = default);
}
