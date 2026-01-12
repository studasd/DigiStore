using DigiStore.UserService.Application.Interfaces;
using DigiStore.UserService.Domain;
using DigiStore.UserService.Domain.Enums;
using DigiStore.UserService.Infrastructure.Postgres.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace DigiStore.UserService.Infrastructure.Postgres.Repositories;


/// <summary>
/// Repository implementation for user data access
/// </summary>
public class UserRepository : IUserRepository
{
	private readonly UserDbContext _context;
	private readonly ILogger<UserRepository> _logger;

	public UserRepository(UserDbContext context, ILogger<UserRepository> logger)
	{
		_context = context;
		_logger = logger;
	}

	public async Task<UserDS?> GetByIdAsync(Guid userId, CancellationToken ct = default)
	{
		return await _context.Users
			.AsNoTracking()
			.FirstOrDefaultAsync(u => u.Id == userId, ct);
	}

	public async Task<UserDS?> GetByEmailAsync(string email, CancellationToken ct = default)
	{
		return await _context.Users
			.AsNoTracking()
			.FirstOrDefaultAsync(u => u.Email == email, ct);
	}

	public async Task<UserDS?> GetByTelegramIdAsync(long telegramId, CancellationToken ct = default)
	{
		return await _context.Users
			.AsNoTracking()
			.FirstOrDefaultAsync(u => u.TelegramId == telegramId, ct);
	}

	public async Task<bool> ExistsByEmailAsync(string email, CancellationToken ct = default)
	{
		return await _context.Users
			.AnyAsync(u => u.Email == email, ct);
	}

	public async Task<bool> ExistsByTelegramIdAsync(long telegramId, CancellationToken ct = default)
	{
		return await _context.Users
			.AnyAsync(u => u.TelegramId == telegramId, ct);
	}

	public async Task AddAsync(UserDS user, CancellationToken ct = default)
	{
		_context.Users.Add(user);
		await _context.SaveChangesAsync(ct);
		_logger.LogInformation("User added: {UserId}", user.Id);
	}

	public async Task UpdateAsync(UserDS user, CancellationToken ct = default)
	{
		_context.Users.Update(user);
		await _context.SaveChangesAsync(ct);
		_logger.LogInformation("User updated: {UserId}", user.Id);
	}

	public async Task DeleteAsync(Guid userId, CancellationToken ct = default)
	{
		var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId, ct);
		if (user != null)
		{
			user.IsDeleted = true;
			_context.Users.Update(user);
			await _context.SaveChangesAsync(ct);
			_logger.LogInformation("User soft deleted: {UserId}", userId);
		}
	}

	public async Task<IEnumerable<UserDS>> GetAllActiveAsync(CancellationToken ct = default)
	{
		return await _context.Users
			.AsNoTracking()
			.Where(u => u.IsActive && !u.IsDeleted)
			.ToListAsync(ct);
	}

	public async Task<IEnumerable<UserDS>> GetBySourceAsync(UserSource source, CancellationToken ct = default)
	{
		return await _context.Users
			.AsNoTracking()
			.Where(u => u.Source == source)
			.ToListAsync(ct);
	}
}
