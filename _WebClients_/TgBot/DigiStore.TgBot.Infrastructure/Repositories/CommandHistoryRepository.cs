using CSharpFunctionalExtensions;
using DigiStore.SharedKernel;
using DigiStore.TgBot.Application.Interfaces.Repositories;
using DigiStore.TgBot.Domain;
using DigiStore.TgBot.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DigiStore.TgBot.Infrastructure.Repositories;

public class CommandHistoryRepository : ICommandHistoryRepository
{
    private readonly TgBotDbContext _db;
    private readonly ILogger<CommandHistoryRepository> _logger;

    public CommandHistoryRepository(TgBotDbContext db, ILogger<CommandHistoryRepository> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<UnitResult<Error>> AddAsync(CommandHistory history, CancellationToken token)
    {
        _db.CommandHistories.Add(history);

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
