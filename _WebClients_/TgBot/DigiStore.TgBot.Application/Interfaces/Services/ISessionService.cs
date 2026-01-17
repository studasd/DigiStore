using CSharpFunctionalExtensions;
using DigiStore.SharedKernel;
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
	Task<Result<TgUserSession, Error>> GetOrCreateSessionAsync(long telegramId, CancellationToken token);

	/// <summary>
	/// Update session
	/// </summary>
	Task UpdateSessionAsync(TgUserSession session, CancellationToken token);

	/// <summary>
	/// Clear session
	/// </summary>
	Task ClearSessionAsync(long telegramId, CancellationToken token);

	/// <summary>
	/// Get session
	/// </summary>
	Task<Result<TgUserSession, Error>> GetSessionAsync(long telegramId, CancellationToken token);

	/// <summary>
	/// Record command or callback history for a telegram user
	/// </summary>
	Task RecordCommandAsync(long telegramId, string command, string? message = null, CancellationToken token = default);
}
