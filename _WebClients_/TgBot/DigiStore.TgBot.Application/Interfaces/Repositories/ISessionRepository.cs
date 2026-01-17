using DigiStore.TgBot.Domain;

namespace DigiStore.TgBot.Application.Interfaces.Repositories;

public interface ISessionRepository
{
    Task<TgUserSession?> GetByTelegramIdAsync(long telegramId, CancellationToken token);
    Task AddOrUpdateAsync(TgUserSession session, CancellationToken token);
    Task DeleteByTelegramIdAsync(long telegramId, CancellationToken token);
}
