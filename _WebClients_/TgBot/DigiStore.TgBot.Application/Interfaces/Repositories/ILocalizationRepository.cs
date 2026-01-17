using DigiStore.TgBot.Domain;

namespace DigiStore.TgBot.Application.Interfaces.Repositories;

public interface ILocalizationRepository
{
    Task<Localization?> GetByKeyAsync(string key, CancellationToken token);
    Task<IEnumerable<Localization>> GetAllAsync(CancellationToken token);
    Task AddOrUpdateAsync(Localization entity, CancellationToken token);
}
