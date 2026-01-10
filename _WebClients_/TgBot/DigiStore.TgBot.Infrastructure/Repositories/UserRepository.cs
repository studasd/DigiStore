using DigiStore.TgBot.Application.Interfaces.Repositories;
using DigiStore.TgBot.Domain;
using DigiStore.TgBot.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DigiStore.TgBot.Infrastructure.Repositories;

public class UserRepository : ITelegramUserRepository
{
    private readonly TgBotDbContext _db;

    public UserRepository(TgBotDbContext db)
    {
        _db = db;
    }

    public async Task<TgUser?> GetByTelegramIdAsync(long telegramId, CancellationToken ct = default)
    {
        return await _db.TelegramUsers.FirstOrDefaultAsync(u => u.TelegramId == telegramId, ct);
    }

    public async Task<TgUser?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _db.TelegramUsers.FirstOrDefaultAsync(u => u.Id == id, ct);
    }

    public async Task AddOrUpdateAsync(TgUser user, CancellationToken ct = default)
    {
        var existing = await _db.TelegramUsers.FirstOrDefaultAsync(u => u.TelegramId == user.TelegramId, ct);
        if (existing == null)
        {
            _db.TelegramUsers.Add(user);
        }
        else
        {
            existing.FirstName = user.FirstName;
            existing.LastName = user.LastName;
            existing.Username = user.Username;
            existing.IsActive = user.IsActive;
            existing.UpdatedAt = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync(ct);
    }
}
