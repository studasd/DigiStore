using DigiStore.TgBot.Application.Interfaces.Services;
using Microsoft.Extensions.Logging;
using StudTgBotApi.Contracts.Interfaces;

namespace DigiStore.TgBot.Application.Services;

public class UserStateProvider : IUserStateProvider
{
	private readonly ISessionService _sessionService;
	private readonly ILogger<UserStateProvider> _logger;

	public UserStateProvider(
		ISessionService sessionService,
		ILogger<UserStateProvider> logger)
	{
		_sessionService = sessionService;
		_logger = logger;
	}

	public async Task<string?> GetUserStateAsync(long userId, CancellationToken cancellationToken = default)
	{
		var sessionResult = await _sessionService.GetSessionAsync(userId, cancellationToken);
		
		if (sessionResult.IsFailure)
		{
			_logger.LogDebug("Session not found for user {UserId}, returning null state", userId);
			return null;
		}

		var state = sessionResult.Value.CurrentState;
		_logger.LogDebug("Retrieved state '{State}' for user {UserId}", state, userId);
		
		return string.IsNullOrWhiteSpace(state) ? null : state;
	}

	public async Task SetUserStateAsync(long userId, string state, CancellationToken cancellationToken = default)
	{
		var sessionResult = await _sessionService.GetOrCreateSessionAsync(userId, cancellationToken);
		
		if (sessionResult.IsFailure)
		{
			_logger.LogError("Failed to get or create session for user {UserId}: {Error}", userId, sessionResult.Error);
			return;
		}

		var session = sessionResult.Value;
		session.SetState(state);
		
		var updateResult = await _sessionService.UpdateSessionAsync(session, cancellationToken);
		
		if (updateResult.IsFailure)
		{
			_logger.LogError("Failed to update session for user {UserId}: {Error}", userId, updateResult.Error);
		}
		else
		{
			_logger.LogDebug("Set state '{State}' for user {UserId}", state, userId);
		}
	}

	public async Task ClearUserStateAsync(long userId, CancellationToken cancellationToken = default)
	{
		var sessionResult = await _sessionService.GetSessionAsync(userId, cancellationToken);
		
		if (sessionResult.IsFailure)
		{
			_logger.LogDebug("No session found to clear for user {UserId}", userId);
			return;
		}

		var session = sessionResult.Value;
		session.SetState(string.Empty);
		
		var updateResult = await _sessionService.UpdateSessionAsync(session, cancellationToken);
		
		if (updateResult.IsFailure)
		{
			_logger.LogError("Failed to clear state for user {UserId}: {Error}", userId, updateResult.Error);
		}
		else
		{
			_logger.LogDebug("Cleared state for user {UserId}", userId);
		}
	}
}
