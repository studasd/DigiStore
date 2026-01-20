using CSharpFunctionalExtensions;
using DigiStore.SharedKernel;
using DigiStore.TgBot.Application.Constants;
using DigiStore.TgBot.Application.Interfaces.Repositories;
using DigiStore.TgBot.Application.Interfaces.Services;
using DigiStore.TgBot.Domain;
using Microsoft.Extensions.Logging;

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

    public async Task<Result<TgUserSession, Error>> GetOrCreateSessionAsync(long telegramId, CancellationToken token)
    {
        var domainResult = await _sessionRepository.GetByTelegramIdAsync(telegramId, token);
        if(domainResult.IsSuccess)
        {
            var session = MapToDomain(domainResult.Value);
            _logger.LogDebug("Session retrieved from repository for Telegram ID: {TelegramId}", telegramId);
            return session;
        }

        var newSession = new TgUserSession
        {
            TelegramId = telegramId,
            CurrentState = BotState.Start,
            CreatedAt = DateTime.UtcNow
        };

        var resultSave = await SaveSessionAsync(newSession, token);
        if (resultSave.IsFailure)
            return resultSave.Error;

		_logger.LogInformation("New session created for Telegram ID: {TelegramId}", telegramId);

        return newSession;
    }


    public async Task<UnitResult<Error>> UpdateSessionAsync(TgUserSession session, CancellationToken token)
    {
        session.UpdateActivity();
        return await SaveSessionAsync(session, token);
    }


    public async Task<UnitResult<Error>> ClearSessionAsync(long telegramId, CancellationToken token)
    {
        var seeDetelResult = await _sessionRepository.DeleteByTelegramIdAsync(telegramId, token);
        if (seeDetelResult.IsFailure)
            return seeDetelResult.Error;
        _logger.LogInformation("Session cleared for Telegram ID: {TelegramId}", telegramId);
        return Result.Success<Error>();
    }


    public async Task<Result<TgUserSession, Error>> GetSessionAsync(long telegramId, CancellationToken token)
    {
        var domainResult = await _sessionRepository.GetByTelegramIdAsync(telegramId, token);
        if (domainResult.IsFailure)
            return domainResult.Error;
        
        return MapToDomain(domainResult.Value);
    }


    private async Task<UnitResult<Error>> SaveSessionAsync(TgUserSession session, CancellationToken token)
    {
        // convert to domain.UserSession and save via repository
        var domain = MapToDomainModel(session);
        return await _sessionRepository.AddOrUpdateAsync(domain, token);
    }

    private TgUserSession MapToDomain(TgUserSession d)
    {
        var session = new TgUserSession
        {
            Id = d.Id,
            UserId = d.UserId,
            TelegramId = d.TelegramId,
            CurrentState = d.CurrentState,
            LangCode = d.LangCode,
            CreatedAt = d.CreatedAt,
            LastActivity = d.LastActivity,
			CachedProfile = d.CachedProfile,
			PendingTopUpAggregator = d.PendingTopUpAggregator,
			PendingTopUpAmount = d.PendingTopUpAmount,
			PendingTopUpChatId = d.PendingTopUpChatId,
			PendingTopUpMessageId = d.PendingTopUpMessageId
		};

        return session;
    }

    private TgUserSession MapToDomainModel(TgUserSession s)
    {
        return new TgUserSession
        {
            Id = s.Id == Guid.Empty ? Guid.NewGuid() : s.Id,
            UserId = s.UserId,
            TelegramId = s.TelegramId,
            CurrentState = s.CurrentState,
            LangCode = s.LangCode,
            CachedProfile = s.CachedProfile,
			PendingTopUpAggregator = s.PendingTopUpAggregator,
			PendingTopUpAmount = s.PendingTopUpAmount,
			PendingTopUpChatId = s.PendingTopUpChatId,
			PendingTopUpMessageId = s.PendingTopUpMessageId,
            LastActivity = s.LastActivity,
            CreatedAt = s.CreatedAt
        };
    }

    // Additional helper to store command history
    public async Task<UnitResult<Error>> RecordCommandAsync(long telegramId, string command, string? message = null, CancellationToken token = default)
    {
        var history = new CommandHistory
        {
            Id = Guid.NewGuid(),
            TelegramId = telegramId,
            Command = command,
            Message = message,
            Timestamp = DateTime.UtcNow
        };

        return await _historyRepository.AddAsync(history, token);
	}
}
