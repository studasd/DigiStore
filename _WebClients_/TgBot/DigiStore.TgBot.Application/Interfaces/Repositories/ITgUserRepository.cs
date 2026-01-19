using DigiStore.TgBot.Domain;

namespace DigiStore.TgBot.Application.Interfaces.Repositories;

public interface ITgUserRepository
{
    Task<TgUser?> GetByTelegramIdAsync(long telegramId, CancellationToken token);
    
	Task<TgUser?> GetByIdAsync(Guid id, CancellationToken token);

	Task<TgUser?> GetByUserIdAsync(Guid userId, CancellationToken token);

	Task AddOrUpdateAsync(TgUser user, CancellationToken token);
}
