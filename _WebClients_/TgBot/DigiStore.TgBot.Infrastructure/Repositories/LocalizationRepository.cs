using DigiStore.TgBot.Application.Interfaces.Repositories;
using DigiStore.TgBot.Domain;
using DigiStore.TgBot.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DigiStore.TgBot.Infrastructure.Repositories;

public class LocalizationRepository : ILocalizationRepository
{
    private readonly TgBotDbContext _db;

    public LocalizationRepository(TgBotDbContext db)
    {
        _db = db;
    }

    public async Task<Localization?> GetByKeyAsync(string key, CancellationToken ct = default)
    {
        return await _db.Localizations.FirstOrDefaultAsync(l => l.Key == key, ct);
    }

    public async Task<IEnumerable<Localization>> GetAllAsync(CancellationToken ct = default)
    {
        return await _db.Localizations.AsNoTracking().ToListAsync(ct);
    }

    public async Task AddOrUpdateAsync(Localization entity, CancellationToken ct = default)
    {
        var existing = await _db.Localizations.FirstOrDefaultAsync(l => l.Key == entity.Key, ct);
        if (existing == null)
        {
            _db.Localizations.Add(entity);
        }
        else
        {
            existing.En = entity.En;
            existing.Ru = entity.Ru;
            existing.UpdatedAt = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync(ct);
    }
}
