using CSharpFunctionalExtensions;
using DigiStore.SharedKernel;
using DigiStore.TgBot.Application.Interfaces.Repositories;
using DigiStore.TgBot.Domain;
using DigiStore.TgBot.Infrastructure.Postgres.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DigiStore.TgBot.Infrastructure.Postgres.Repositories;

public class LocalizationRepository : ILocalizationRepository
{
    private readonly TgBotDbContext _db;
    private readonly ILogger<LocalizationRepository> _logger;

    public LocalizationRepository(TgBotDbContext db, ILogger<LocalizationRepository> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<Result<Localization, Error>> GetByKeyAsync(string key, CancellationToken token)
    {
        var locale = await _db.Localizations.FirstOrDefaultAsync(l => l.Key == key, token);
        if (locale == null)
            return Error.NotFound("localization.notfound", $"Localization with key '{key}' not found");
        return locale;
	}

    public async Task<Result<IEnumerable<Localization>, Error>> GetAllAsync(CancellationToken token)
    {
        return await _db.Localizations.AsNoTracking().ToListAsync(token);
    }

    public async Task<UnitResult<Error>> AddOrUpdateAsync(Localization entity, CancellationToken token)
    {
        var existing = await _db.Localizations.FirstOrDefaultAsync(l => l.Key == entity.Key, token);
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

		return await SaveChangesAsync(token);
	}

	public async Task<UnitResult<Error>> SaveChangesAsync(CancellationToken token)
	{
		try
		{
			await _db.SaveChangesAsync(token);
		}
		catch (DbUpdateException ex)
		{
			_logger.LogWarning(ex, "Failed save changes");

			return Error.Failure("failed.db.savechange", $"Failed save changes");
		}

		return Result.Success<Error>();
	}
}
