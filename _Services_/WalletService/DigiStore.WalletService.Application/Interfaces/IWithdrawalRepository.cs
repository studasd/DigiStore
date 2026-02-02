using CSharpFunctionalExtensions;
using DigiStore.Enums;
using StudCoreKit.SharedKernel;
using DigiStore.WalletService.Domain;

namespace DigiStore.WalletService.Application.Interfaces;

public interface IWithdrawalRepository
{
	Task<Result<WithdrawalDS, Error>> AddAsync(WithdrawalDS withdrawal, CancellationToken token);

	Task<Result<WithdrawalDS, Error>> GetByIdAsync(Guid withdrawalId, CancellationToken token);

	Task<Result<WithdrawalDS, Error>> GetByAggregatorIdAsync(string aggregatorWithdrawalId, CancellationToken token);

	Task<Result<List<WithdrawalDS>, Error>> GetUserWithdrawalsAsync(Guid userId, int skip = 0, int take = 10, CancellationToken token = default);

	Task<UnitResult<Error>> UpdateAsync(WithdrawalDS withdrawal, CancellationToken token);

	Task<UnitResult<Error>> UpdateWithdrawalStatusAsync(Guid withdrawalId, WithdrawalStatus status, CancellationToken token);

	Task<UnitResult<Error>> CompleteWithdrawalAsync(Guid withdrawalId, CancellationToken token);

	Task<UnitResult<Error>> SaveChangesAsync(CancellationToken token);
}
