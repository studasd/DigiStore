using DigiStore.TgBot.Domain;

namespace DigiStore.TgBot.Application.Interfaces.Repositories;

public interface ITgUserRepository
{
    Task<TgUser?> GetByTelegramIdAsync(long telegramId, CancellationToken ct = default);
    Task<TgUser?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task AddOrUpdateAsync(TgUser user, CancellationToken ct = default);
}
