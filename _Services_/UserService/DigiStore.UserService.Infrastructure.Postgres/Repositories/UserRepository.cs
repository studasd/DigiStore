using CSharpFunctionalExtensions;
using DigiStore.SharedKernel;
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

	public async Task<Result<UserDS,Error>> GetByIdAsync(Guid userId, CancellationToken token)
	{
		var user = await _context.Users
			.AsNoTracking()
			.FirstOrDefaultAsync(u => u.Id == userId, token);

		if (user == null)
			return Error.NotFound("user.not_found", $"Нет пользователя с ID: {userId}");

		return user;
	}

	public async Task<Result<UserDS, Error>> GetByEmailAsync(string email, CancellationToken token)
	{
		var user = await _context.Users
			.AsNoTracking()
			.FirstOrDefaultAsync(u => u.Email == email, token);

		if (user == null)
			return Error.NotFound("user.not_found", $"Нет пользователя с Email: {email}");

		return user;
	}

	public async Task<Result<UserDS, Error>> GetByTelegramIdAsync(long telegramId, CancellationToken token)
	{
		var user = await _context.Users
			.AsNoTracking()
			.FirstOrDefaultAsync(u => u.TelegramId == telegramId, token);

		if (user == null)
			return Error.NotFound("user.not_found", $"Нет пользователя с Telegram Id: {telegramId}");

		return user;
	}

	public async Task<Result<bool, Error>> ExistsByEmailAsync(string email, CancellationToken token)
	{
		return await _context.Users
			.AnyAsync(u => u.Email == email, token);
	}

	public async Task<Result<bool, Error>> ExistsByTelegramIdAsync(long telegramId, CancellationToken token)
	{
		return await _context.Users
			.AnyAsync(u => u.TelegramId == telegramId, token);
	}

	public async Task<UnitResult<Error>> AddAsync(UserDS user, CancellationToken token)
	{
		_context.Users.Add(user);

		var saveResult = await SaveChangesAsync(token);
		if (saveResult.IsFailure)
			return saveResult.Error;

		_logger.LogInformation("User added: {UserId}", user.Id);
		return Result.Success<Error>();
	}

	public async Task<UnitResult<Error>> UpdateAsync(UserDS user, CancellationToken token)
	{
		_context.Users.Update(user);

		var saveResult = await SaveChangesAsync(token);
		if (saveResult.IsFailure)
			return saveResult.Error;

		_logger.LogInformation("User updated: {UserId}", user.Id);
		return Result.Success<Error>();
	}

	public async Task<UnitResult<Error>> DeleteAsync(Guid userId, CancellationToken token)
	{
		var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId, token);
		if (user == null)
			return Error.NotFound("user.not_found", $"Нет пользователя с ID: {userId}");

		user.IsDeleted = true;
		_context.Users.Update(user);

		var saveResult = await SaveChangesAsync(token);
		if (saveResult.IsFailure)
			return saveResult.Error;

		_logger.LogInformation("User soft deleted: {UserId}", userId);
		return Result.Success<Error>();
	}

	public async Task<Result<IEnumerable<UserDS>, Error>> GetAllActiveAsync(CancellationToken token)
	{
		return await _context.Users
			.AsNoTracking()
			.Where(u => u.IsActive && !u.IsDeleted)
			.ToListAsync(token);
	}

	public async Task<Result<IEnumerable<UserDS>, Error>> GetBySourceAsync(UserSource source, CancellationToken token)
	{
		return await _context.Users
			.AsNoTracking()
			.Where(u => u.Source == source)
			.ToListAsync(token);
	}


	public async Task<UnitResult<Error>> SaveChangesAsync(CancellationToken token)
	{
		try
		{
			await _context.SaveChangesAsync(token);
		}
		catch (DbUpdateException ex)
		{
			_logger.LogWarning(ex, "Failed save changes");

			return Error.Failure("failed.db.savechange", $"Failed save changes");
		}

		return Result.Success<Error>();
	}
}
