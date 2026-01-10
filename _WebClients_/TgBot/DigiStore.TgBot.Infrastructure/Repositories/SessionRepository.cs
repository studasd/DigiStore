using DigiStore.TgBot.Application.Interfaces.Repositories;
using DigiStore.TgBot.Domain;
using DigiStore.TgBot.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DigiStore.TgBot.Infrastructure.Repositories;

public class SessionRepository : ITelegramSessionRepository
{
    private readonly TgBotDbContext _db;

    public SessionRepository(TgBotDbContext db)
    {
        _db = db;
    }

    public async Task<TgUserSession?> GetByTelegramIdAsync(long telegramId, CancellationToken ct = default)
    {
        return await _db.TelegramSessions.FirstOrDefaultAsync(s => s.TelegramId == telegramId, ct);
    }

    public async Task AddOrUpdateAsync(TgUserSession session, CancellationToken ct = default)
    {
        var existing = await _db.TelegramSessions.FirstOrDefaultAsync(s => s.TelegramId == session.TelegramId, ct);
        if (existing == null)
        {
            _db.TelegramSessions.Add(session);
        }
        else
        {
            existing.CurrentState = session.CurrentState;
            existing.LanguageCode = session.LanguageCode;
            existing.Data = session.Data;
            existing.CachedProfile = session.CachedProfile;
            existing.LastActivity = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync(ct);
    }

    public async Task DeleteByTelegramIdAsync(long telegramId, CancellationToken ct = default)
    {
        var existing = await _db.TelegramSessions.FirstOrDefaultAsync(s => s.TelegramId == telegramId, ct);
        if (existing != null)
        {
            _db.TelegramSessions.Remove(existing);
            await _db.SaveChangesAsync(ct);
        }
    }
}
