using CSharpFunctionalExtensions;
using DigiStore.Framework.Endpoints;
using DigiStore.SharedKernel;
using DigiStore.UserService.Application.Interfaces;
using DigiStore.UserService.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;

namespace DigiStore.UserService.Application.Features.Roles;

public sealed class AssignRole : IEndpoint
{
	[Authorize(Roles = "Admin")]
	public void MapEndpoint(IEndpointRouteBuilder app)
	{
		app.MapPost("role/{userId}/{roleName}", async Task<EndpointResult<bool>> (
			[FromRoute] Guid userId,
			[FromRoute] string roleName,
			[FromServices] AssignRoleHandler handler,
			CancellationToken token) => await handler.Handle(userId, roleName, token));
	}
}

public sealed class AssignRoleHandler
{
	private readonly UserManager<UserDS> _userManager;
	private readonly RoleManager<RoleDS> _roleManager;
	private readonly ILogger<AssignRoleHandler> _logger;
	private readonly IUserRepository _userRepository;

	public AssignRoleHandler(
		UserManager<UserDS> userManager,
		RoleManager<RoleDS> roleManager,
		ILogger<AssignRoleHandler> logger,
		IUserRepository userRepository)
	{
		_userManager = userManager;
		_roleManager = roleManager;
		_logger = logger;
		_userRepository = userRepository;
	}


	public async Task<Result<bool, Error>> Handle(Guid userId, string roleName, CancellationToken token)
	{
		try
		{
			var user = await _userRepository.GetByIdAsync(userId, token);
			if (user == null)
			{
				return UserServiceErrors.UserNotFound;
			}

			var role = await _roleManager.FindByNameAsync(roleName);
			if (role == null)
			{
				return UserServiceErrors.RoleNotFound;
			}

			var result = await _userManager.AddToRoleAsync(user, roleName);
			if (!result.Succeeded)
			{
				var errors = string.Join("; ", result.Errors.Select(e => e.Description));
				return Error.Failure("role.assignment_failed", errors);
			}

			_logger.LogInformation("Role {RoleName} assigned to user {UserId}", roleName, userId);

			return true;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error assigning role to user: {UserId}, {RoleName}", userId, roleName);
			return Error.Failure("role.assignment_error", ex.Message);
		}
	}
}