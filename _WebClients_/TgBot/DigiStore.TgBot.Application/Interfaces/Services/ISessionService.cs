using CSharpFunctionalExtensions;
using DigiStore.SharedKernel;
using DigiStore.TgBot.Domain;

namespace DigiStore.TgBot.Application.Interfaces.Services;


/// <summary>
/// Service to manage user sessions
/// </summary>
public interface ISessionService
{
	Task<Result<TgSession, Error>> GetOrCreateSessionAsync(long telegramId, CancellationToken token);

	Task<UnitResult<Error>> UpdateSessionAsync(TgSession session, CancellationToken token);

	Task<UnitResult<Error>> ClearSessionAsync(long telegramId, CancellationToken token);

	Task<Result<TgSession, Error>> GetSessionAsync(long telegramId, CancellationToken token);

	Task<UnitResult<Error>> RecordCommandAsync(long telegramId, string command, string? message = null, CancellationToken token = default);
}
