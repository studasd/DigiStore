using CSharpFunctionalExtensions;
using DigiStore.SharedKernel;
using DigiStore.TgBot.Domain;

namespace DigiStore.TgBot.Application.Interfaces.Repositories;

public interface ITgUserRepository
{
	Task<Result<TgUser, Error>> GetByTelegramIdAsync(long telegramId, CancellationToken token);

	Task<Result<TgUser, Error>> GetByUserIdAsync(Guid userId, CancellationToken token);

	Task<UnitResult<Error>> AddOrUpdateAsync(TgUser user, CancellationToken token);

	Task<UnitResult<Error>> SaveChangesAsync(CancellationToken token);
}
