using DigiStore.TgBot.Domain;

namespace DigiStore.TgBot.Application.Interfaces.Repositories;

public interface ISessionRepository
{
    Task<TgUserSession?> GetByTelegramIdAsync(long telegramId, CancellationToken ct = default);
    Task AddOrUpdateAsync(TgUserSession session, CancellationToken ct = default);
    Task DeleteByTelegramIdAsync(long telegramId, CancellationToken ct = default);
}
