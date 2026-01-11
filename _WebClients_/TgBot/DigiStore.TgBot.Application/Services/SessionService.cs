using DigiStore.TgBot.Application.Constants;
using DigiStore.TgBot.Application.Interfaces.Repositories;
using DigiStore.TgBot.Application.Interfaces.Services;
using DigiStore.TgBot.Domain;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace DigiStore.TgBot.Application.Services;


public class SessionService : ISessionService
{
    private readonly ISessionRepository _sessionRepository;
    private readonly ICommandHistoryRepository _historyRepository;
    private readonly ILogger<SessionService> _logger;
    private const string SessionKeyFormat = "tg:session:{0}";
    private readonly TimeSpan _sessionExpiration = TimeSpan.FromHours(24);

    public SessionService(
        ISessionRepository sessionRepository,
        ICommandHistoryRepository historyRepository,
        ILogger<SessionService> logger)
    {
        _sessionRepository = sessionRepository;
        _historyRepository = historyRepository;
        _logger = logger;
    }

    public async Task<TgUserSession> GetOrCreateSessionAsync(long telegramId, CancellationToken ct = default)
    {
        var domain = await _sessionRepository.GetByTelegramIdAsync(telegramId, ct);
        if (domain != null)
        {
            var session = MapToDomain(domain);
            _logger.LogDebug("Session retrieved from repository for Telegram ID: {TelegramId}", telegramId);
            return session;
        }

        var newSession = new TgUserSession
        {
            TelegramId = telegramId,
            CurrentState = BotState.Start,
            CreatedAt = DateTime.UtcNow
        };

        await SaveSessionAsync(newSession, ct);
        _logger.LogInformation("New session created for Telegram ID: {TelegramId}", telegramId);

        return newSession;
    }


    public async Task UpdateSessionAsync(TgUserSession session, CancellationToken ct = default)
    {
        session.UpdateActivity();
        await SaveSessionAsync(session, ct);
    }


    public async Task ClearSessionAsync(long telegramId, CancellationToken ct = default)
    {
        await _sessionRepository.DeleteByTelegramIdAsync(telegramId, ct);
        _logger.LogInformation("Session cleared for Telegram ID: {TelegramId}", telegramId);
    }


    public async Task<TgUserSession?> GetSessionAsync(long telegramId, CancellationToken ct = default)
    {
        var domain = await _sessionRepository.GetByTelegramIdAsync(telegramId, ct);
        if (domain == null)
            return null;
        return MapToDomain(domain);
    }


    private async Task SaveSessionAsync(TgUserSession session, CancellationToken ct)
    {
        // convert to domain.UserSession and save via repository
        var domain = MapToDomainModel(session);
        await _sessionRepository.AddOrUpdateAsync(domain, ct);
    }

    private TgUserSession MapToDomain(TgUserSession d)
    {
        var session = new TgUserSession
        {
            TelegramId = d.TelegramId,
            CurrentState = d.CurrentState,
            LangCode = d.LangCode,
            CreatedAt = d.CreatedAt,
            LastActivity = d.LastActivity,
            CachedProfile = d.CachedProfile
		};

        return session;
    }

    private TgUserSession MapToDomainModel(TgUserSession s)
    {
        return new TgUserSession
        {
            Id = Guid.NewGuid(),
            TelegramId = s.TelegramId,
            CurrentState = s.CurrentState,
            LangCode = s.LangCode ?? "en",
            CachedProfile = s.CachedProfile,
            LastActivity = s.LastActivity,
            CreatedAt = s.CreatedAt
        };
    }

    // Additional helper to store command history
    public async Task RecordCommandAsync(long telegramId, string command, object? payload = null, CancellationToken ct = default)
    {
        var history = new CommandHistory
        {
            Id = Guid.NewGuid(),
            TelegramId = telegramId,
            Command = command,
            Timestamp = DateTime.UtcNow
        };

        await _historyRepository.AddAsync(history, ct);
    }
}
