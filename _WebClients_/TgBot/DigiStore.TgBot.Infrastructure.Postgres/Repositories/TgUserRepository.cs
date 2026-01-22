using CSharpFunctionalExtensions;
using DigiStore.SharedKernel;
using DigiStore.TgBot.Application.Interfaces.Repositories;
using DigiStore.TgBot.Domain;
using DigiStore.TgBot.Infrastructure.Postgres.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DigiStore.TgBot.Infrastructure.Postgres.Repositories;

public class TgUserRepository : ITgUserRepository
{
    private readonly TgBotDbContext _db;
    private readonly ILogger<TgUserRepository> _logger;

    public TgUserRepository(TgBotDbContext db, ILogger<TgUserRepository> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<Result<TgUser, Error>> GetByTelegramIdAsync(long telegramId, CancellationToken token)
    {
        var user = await _db.TelegramUsers.FirstOrDefaultAsync(u => u.TelegramId == telegramId, token);
        if(user == null)
            return Error.NotFound("tguser.notfound", $"Telegram user with TelegramId '{telegramId}' not found");

        return user;
	}

	public async Task<Result<TgUser, Error>> GetByUserIdAsync(Guid userId, CancellationToken token)
	{
		var user = await _db.TelegramUsers.FirstOrDefaultAsync(u => u.UserId == userId, token);
        if(user == null)
            return Error.NotFound("tguser.notfound", $"Telegram user with UserId '{userId}' not found");

        return user;
	}

	public async Task<UnitResult<Error>> AddOrUpdateAsync(TgUser user, CancellationToken token)
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
