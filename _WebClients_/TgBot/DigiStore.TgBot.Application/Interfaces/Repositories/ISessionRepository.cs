using CSharpFunctionalExtensions;
using DigiStore.SharedKernel;
using DigiStore.TgBot.Domain;

namespace DigiStore.TgBot.Application.Interfaces.Repositories;

public interface ISessionRepository
{
	Task<Result<TgUserSession, Error>> GetByTelegramIdAsync(long telegramId, CancellationToken token);

	Task<UnitResult<Error>> AddOrUpdateAsync(TgUserSession session, CancellationToken token);

	Task<UnitResult<Error>> DeleteByTelegramIdAsync(long telegramId, CancellationToken token);

	Task<UnitResult<Error>> SaveChangesAsync(CancellationToken token);
}
