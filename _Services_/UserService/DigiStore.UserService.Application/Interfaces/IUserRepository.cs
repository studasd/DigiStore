using CSharpFunctionalExtensions;
using DigiStore.SharedKernel;
using DigiStore.UserService.Domain;
using DigiStore.UserService.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace DigiStore.UserService.Application.Interfaces;


/// <summary>
/// Repository for user data access
/// </summary>
public interface IUserRepository
{
	Task<Result<UserDS, Error>> GetByIdAsync(Guid userId, CancellationToken token);

	Task<Result<UserDS, Error>> GetByEmailAsync(string email, CancellationToken token);

	Task<Result<UserDS, Error>> GetByTelegramIdAsync(long telegramId, CancellationToken token);

	Task<Result<bool, Error>> ExistsByEmailAsync(string email, CancellationToken token);

	Task<Result<bool, Error>> ExistsByTelegramIdAsync(long telegramId, CancellationToken token);

	Task<UnitResult<Error>> AddAsync(UserDS user, CancellationToken token);

	Task<UnitResult<Error>> UpdateAsync(UserDS user, CancellationToken token);

	Task<UnitResult<Error>> DeleteAsync(Guid userId, CancellationToken token);

	Task<Result<IEnumerable<UserDS>, Error>> GetAllActiveAsync(CancellationToken token);

	Task<Result<IEnumerable<UserDS>, Error>> GetBySourceAsync(UserSource source, CancellationToken token);

	Task<UnitResult<Error>> SaveChangesAsync(CancellationToken token);
}
