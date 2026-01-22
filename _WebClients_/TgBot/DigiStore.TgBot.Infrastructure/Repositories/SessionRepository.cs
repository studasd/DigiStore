using CSharpFunctionalExtensions;
using DigiStore.SharedKernel;
using DigiStore.TgBot.Application.Interfaces.Repositories;
using DigiStore.TgBot.Domain;
using DigiStore.TgBot.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DigiStore.TgBot.Infrastructure.Repositories;

public class SessionRepository : ISessionRepository
{
    private readonly TgBotDbContext _db;
    private readonly ILogger<SessionRepository> _logger;

    public SessionRepository(TgBotDbContext db, ILogger<SessionRepository> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<Result<TgSession,Error>> GetByTelegramIdAsync(long telegramId, CancellationToken token)
    {
        var session = await _db.TelegramSessions.FirstOrDefaultAsync(s => s.TelegramId == telegramId, token);
        if(session == null)
            return Error.Failure("session.notfound", $"Session with TelegramId {telegramId} not found");

        return session;
	}

    public async Task<UnitResult<Error>> AddOrUpdateAsync(TgSession session, CancellationToken token)
    {
        var existing = await _db.TelegramSessions.FirstOrDefaultAsync(s => s.TelegramId == session.TelegramId, token);
        if (existing == null)
        {
            _db.TelegramSessions.Add(session);
        }
        else
        {
            existing.UserId = session.UserId; // ensure UserId is updated when session is updated
            existing.CurrentState = session.CurrentState;
            existing.LangCode = session.LangCode;
            existing.CachedProfile = session.CachedProfile;
			// Assign new dictionary instances to avoid reference-equality issues in EF change tracking
			existing.PendingPayments = session.PendingPayments is null
				? new Dictionary<Guid, Domain.ValueObjects.PendingPaymentMessageVO>()
				: new Dictionary<Guid, Domain.ValueObjects.PendingPaymentMessageVO>(session.PendingPayments);
			existing.MessageContexts = session.MessageContexts is null
				? new Dictionary<string, Domain.ValueObjects.MessageContextVO>()
				: new Dictionary<string, Domain.ValueObjects.MessageContextVO>(session.MessageContexts);
			existing.LastActivity = DateTime.UtcNow;
        }

		return await SaveChangesAsync(token);
	}

    public async Task<UnitResult<Error>> DeleteByTelegramIdAsync(long telegramId, CancellationToken token)
    {
        var existing = await _db.TelegramSessions.FirstOrDefaultAsync(s => s.TelegramId == telegramId, token);
        if (existing == null)
            return Error.Failure("session.notfound", $"Session with TelegramId {telegramId} not found");

		_db.TelegramSessions.Remove(existing);

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
