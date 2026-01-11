using DigiStore.TgBot.Domain;

namespace DigiStore.TgBot.Application.Interfaces.Services;


/// <summary>
/// Service to manage user sessions
/// </summary>
public interface ISessionService
{
	/// <summary>
	/// Get or create session
	/// </summary>
	Task<TgUserSession> GetOrCreateSessionAsync(long telegramId, CancellationToken ct = default);

	/// <summary>
	/// Update session
	/// </summary>
	Task UpdateSessionAsync(TgUserSession session, CancellationToken ct = default);

	/// <summary>
	/// Clear session
	/// </summary>
	Task ClearSessionAsync(long telegramId, CancellationToken ct = default);

	/// <summary>
	/// Get session
	/// </summary>
	Task<TgUserSession?> GetSessionAsync(long telegramId, CancellationToken ct = default);
}
