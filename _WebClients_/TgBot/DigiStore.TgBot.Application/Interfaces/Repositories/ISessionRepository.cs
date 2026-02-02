using CSharpFunctionalExtensions;
using StudCoreKit.SharedKernel;
using DigiStore.TgBot.Domain;

namespace DigiStore.TgBot.Application.Interfaces.Repositories;

public interface ISessionRepository
{
	Task<Result<TgSession, Error>> GetByTelegramIdAsync(long telegramId, CancellationToken token);

	Task<UnitResult<Error>> AddOrUpdateAsync(TgSession session, CancellationToken token);

	Task<UnitResult<Error>> DeleteByTelegramIdAsync(long telegramId, CancellationToken token);

	Task<UnitResult<Error>> SaveChangesAsync(CancellationToken token);
}
