using CSharpFunctionalExtensions;
using DigiStore.UserService.Contracts.Requests;
using DigiStore.UserService.Contracts.Responses;
using System;
using System.Collections.Generic;
using System.Text;

namespace DigiStore.UserService.Application.Interfaces;


/// <summary>
/// Core service for account operations
/// </summary>
public interface IUserService
{
	/// <summary>
	/// Create new user
	/// </summary>
	Task<Result<UserResponse>> CreateUserAsync(CreateUserRequest request, CancellationToken token);

	/// <summary>
	/// Get user by ID
	/// </summary>
	Task<Result<UserResponse>> GetUserByIdAsync(Guid userId, CancellationToken token);

	/// <summary>
	/// Get user by email
	/// </summary>
	Task<Result<UserResponse>> GetUserByEmailAsync(string email, CancellationToken token);

	/// <summary>
	/// Get user by Telegram ID
	/// </summary>
	Task<Result<UserResponse>> GetUserByTelegramIdAsync(long telegramId, CancellationToken token);

	/// <summary>
	/// Unlink Telegram from user
	/// </summary>
	Task<Result> UnlinkTelegramAsync(Guid userId, CancellationToken token);

	/// <summary>
	/// Update user profile
	/// </summary>
	Task<Result<UserResponse>> UpdateProfileAsync(UpdateUserProfileRequest request, CancellationToken token);

	/// <summary>
	/// Update last activity timestamp
	/// </summary>
	Task<Result> UpdateLastActivityAsync(Guid userId, CancellationToken token);

	/// <summary>
	/// Update user language preference
	/// </summary>
	Task<Result> UpdateLanguageAsync(Guid userId, string languageCode, CancellationToken token);

	/// <summary>
	/// Deactivate user
	/// </summary>
	Task<Result> DeactivateUserAsync(Guid userId, CancellationToken token);

	/// <summary>
	/// Activate user
	/// </summary>
	Task<Result> ActivateUserAsync(Guid userId, CancellationToken token);

	/// <summary>
	/// Assign role to user
	/// </summary>
	Task<Result> AssignRoleAsync(Guid userId, string roleName, CancellationToken token);

	/// <summary>
	/// Remove role from user
	/// </summary>
	Task<Result> RemoveRoleAsync(Guid userId, string roleName, CancellationToken token);

	/// <summary>
	/// Get user roles
	/// </summary>
	Task<Result<IEnumerable<string>>> GetUserRolesAsync(Guid userId, CancellationToken token);

	/// <summary>
	/// Check if user has permission
	/// </summary>
	Task<Result<bool>> HasPermissionAsync(Guid userId, string permission, CancellationToken token);
}
