using CSharpFunctionalExtensions;
using StudCoreKit.SharedKernel;
using DigiStore.TgBot.Domain;

namespace DigiStore.TgBot.Application.Interfaces.Repositories;

public interface ICommandHistoryRepository
{
	Task<UnitResult<Error>> AddAsync(CommandHistory history, CancellationToken token);

	Task<UnitResult<Error>> SaveChangesAsync(CancellationToken token);
}
