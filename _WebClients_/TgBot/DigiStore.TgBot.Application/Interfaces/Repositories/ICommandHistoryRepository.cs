using DigiStore.TgBot.Domain;

namespace DigiStore.TgBot.Application.Interfaces.Repositories;

public interface ICommandHistoryRepository
{
    Task AddAsync(CommandHistory history, CancellationToken ct = default);
}
