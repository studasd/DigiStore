using CSharpFunctionalExtensions;
using DigiStore.SharedKernel;
using DigiStore.TgBot.Domain;

namespace DigiStore.TgBot.Application.Interfaces.Services;


/// <summary>
/// Service to manage user sessions
/// </summary>
public interface ISessionService
{
	Task<Result<TgUserSession, Error>> GetOrCreateSessionAsync(long telegramId, CancellationToken token);

	Task<UnitResult<Error>> UpdateSessionAsync(TgUserSession session, CancellationToken token);

	Task<UnitResult<Error>> ClearSessionAsync(long telegramId, CancellationToken token);

	Task<Result<TgUserSession, Error>> GetSessionAsync(long telegramId, CancellationToken token);

	Task<UnitResult<Error>> RecordCommandAsync(long telegramId, string command, string? message = null, CancellationToken token = default);
}
