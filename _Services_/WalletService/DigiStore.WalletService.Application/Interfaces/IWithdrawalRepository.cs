using CSharpFunctionalExtensions;
using DigiStore.SharedKernel;
using DigiStore.WalletService.Domain;

namespace DigiStore.WalletService.Application.Interfaces;

public interface IWithdrawalRepository
{
	Task<Result<WithdrawalDS, Error>> AddAsync(WithdrawalDS withdrawal, CancellationToken ct);

	Task<Result<WithdrawalDS, Error>> GetByIdAsync(Guid withdrawalId, CancellationToken ct);

	Task<Result<WithdrawalDS, Error>> GetByAggregatorIdAsync(string aggregatorWithdrawalId, CancellationToken ct);

	Task<Result<List<WithdrawalDS>, Error>> GetUserWithdrawalsAsync(Guid userId, int skip = 0, int take = 10, CancellationToken ct = default);

	Task<Result<WithdrawalDS, Error>> UpdateAsync(WithdrawalDS withdrawal, CancellationToken ct);

	Task<UnitResult<Error>> SaveChangesAsync(CancellationToken ct);
}
