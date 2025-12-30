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
	Task<UserDS?> GetByIdAsync(Guid userId, CancellationToken ct = default);
	Task<UserDS?> GetByEmailAsync(string email, CancellationToken ct = default);
	Task<UserDS?> GetByTelegramIdAsync(long telegramId, CancellationToken ct = default);
	Task<bool> ExistsByEmailAsync(string email, CancellationToken ct = default);
	Task<bool> ExistsByTelegramIdAsync(long telegramId, CancellationToken ct = default);
	Task AddAsync(UserDS user, CancellationToken ct = default);
	Task UpdateAsync(UserDS user, CancellationToken ct = default);
	Task DeleteAsync(Guid userId, CancellationToken ct = default);
	Task<IEnumerable<UserDS>> GetAllActiveAsync(CancellationToken ct = default);
	Task<IEnumerable<UserDS>> GetBySourceAsync(UserSource source, CancellationToken ct = default);
}
