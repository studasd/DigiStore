using DigiStore.TgBot.Application.Interfaces.Repositories;
using DigiStore.TgBot.Domain;
using DigiStore.TgBot.Infrastructure.Data;

namespace DigiStore.TgBot.Infrastructure.Repositories;

public class CommandHistoryRepository : ICommandHistoryRepository
{
    private readonly TgBotDbContext _db;

    public CommandHistoryRepository(TgBotDbContext db)
    {
        _db = db;
    }

    public async Task AddAsync(CommandHistory history, CancellationToken token)
    {
        _db.CommandHistories.Add(history);
        await _db.SaveChangesAsync(token);
    }
}
