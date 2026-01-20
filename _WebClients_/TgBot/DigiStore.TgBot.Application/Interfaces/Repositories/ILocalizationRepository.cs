using CSharpFunctionalExtensions;
using DigiStore.SharedKernel;
using DigiStore.TgBot.Domain;

namespace DigiStore.TgBot.Application.Interfaces.Repositories;

public interface ILocalizationRepository
{
	Task<Result<Localization, Error>> GetByKeyAsync(string key, CancellationToken token);

	Task<Result<IEnumerable<Localization>, Error>> GetAllAsync(CancellationToken token);

	Task<UnitResult<Error>> AddOrUpdateAsync(Localization entity, CancellationToken token);

	Task<UnitResult<Error>> SaveChangesAsync(CancellationToken token);
}
