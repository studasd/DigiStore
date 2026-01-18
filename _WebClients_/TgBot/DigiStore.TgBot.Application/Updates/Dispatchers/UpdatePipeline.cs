using CSharpFunctionalExtensions;
using DigiStore.SharedKernel;
using DigiStore.TgBot.Application.Interfaces.Services;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Types;

namespace DigiStore.TgBot.Application.Updates.Dispatchers;

public sealed class UpdatePipeline
{
	private readonly IReadOnlyList<IUpdateDispatcher> _dispatchers;
	private readonly ILogger<UpdatePipeline> _logger;
	private readonly ISessionService _sessionService;

	public UpdatePipeline(IEnumerable<IUpdateDispatcher> dispatchers, ISessionService sessionService, ILogger<UpdatePipeline> logger)
	{
		_dispatchers = dispatchers.ToList();
		_sessionService = sessionService;
		_logger = logger;
	}

	public async Task<UnitResult<Error>> DispatchAsync(Update update, CancellationToken token = default)
	{
		await RecordHistoryAsync(update, token);

		foreach (var dispatcher in _dispatchers)
		{
			if (!dispatcher.CanHandle(update))
				continue;

			var result = await dispatcher.DispatchAsync(update, token);
			if (result.IsFailure)
				return result.Error;

			return Result.Success<Error>();
		}

		_logger.LogDebug("No dispatcher matched update type {Type}", update.Type);
		return Result.Success<Error>();
	}

	private async Task RecordHistoryAsync(Update update, CancellationToken token)
	{
		try
		{
			if (update.Message?.From != null)
			{
				await _sessionService.RecordCommandAsync(update.Message.From.Id, null, update.Message.Text, token);
			}
			else if (update.CallbackQuery?.From != null)
			{
				await _sessionService.RecordCommandAsync(update.CallbackQuery.From.Id, update.CallbackQuery.Data, null, token);
			}
		}
		catch (Exception ex)
		{
			_logger.LogWarning(ex, "Failed to record history in pipeline");
		}
	}
}
