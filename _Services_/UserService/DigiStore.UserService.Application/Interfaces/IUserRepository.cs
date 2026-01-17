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
	Task<UserDS?> GetByIdAsync(Guid userId, CancellationToken token);
	Task<UserDS?> GetByEmailAsync(string email, CancellationToken token);
	Task<UserDS?> GetByTelegramIdAsync(long telegramId, CancellationToken token);
	Task<bool> ExistsByEmailAsync(string email, CancellationToken token);
	Task<bool> ExistsByTelegramIdAsync(long telegramId, CancellationToken token);
	Task AddAsync(UserDS user, CancellationToken token);
	Task UpdateAsync(UserDS user, CancellationToken token);
	Task DeleteAsync(Guid userId, CancellationToken token);
	Task<IEnumerable<UserDS>> GetAllActiveAsync(CancellationToken token);
	Task<IEnumerable<UserDS>> GetBySourceAsync(UserSource source, CancellationToken token);
}
