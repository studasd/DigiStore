using DigiStore.TgBot.Application.Interfaces.Repositories;
using DigiStore.TgBot.Domain;
using DigiStore.TgBot.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DigiStore.TgBot.Infrastructure.Repositories;

public class TgUserRepository : ITgUserRepository
{
    private readonly TgBotDbContext _db;

    public TgUserRepository(TgBotDbContext db)
    {
        _db = db;
    }

    public async Task<TgUser?> GetByTelegramIdAsync(long telegramId, CancellationToken token)
    {
        return await _db.TelegramUsers.FirstOrDefaultAsync(u => u.TelegramId == telegramId, token);
    }

    public async Task<TgUser?> GetByIdAsync(Guid id, CancellationToken token)
    {
        return await _db.TelegramUsers.FirstOrDefaultAsync(u => u.Id == id, token);
    }

	public async Task<TgUser?> GetByUserIdAsync(Guid userId, CancellationToken token)
	{
		return await _db.TelegramUsers.FirstOrDefaultAsync(u => u.UserId == userId, token);
	}

	public async Task AddOrUpdateAsync(TgUser user, CancellationToken token)
    {
        var existing = await _db.TelegramUsers.FirstOrDefaultAsync(u => u.TelegramId == user.TelegramId, token);
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

        await _db.SaveChangesAsync(token);
    }
}
