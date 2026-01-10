using DigiStore.TgBot.Domain;

namespace DigiStore.TgBot.Application.Interfaces.Repositories;

public interface ILocalizationRepository
{
    Task<Localization?> GetByKeyAsync(string key, CancellationToken ct = default);
    Task<IEnumerable<Localization>> GetAllAsync(CancellationToken ct = default);
    Task AddOrUpdateAsync(Localization entity, CancellationToken ct = default);
}
