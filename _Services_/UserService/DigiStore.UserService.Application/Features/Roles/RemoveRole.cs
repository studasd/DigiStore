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

public sealed class RemoveRole : IEndpoint
{
	[Authorize(Roles = "Admin")]
	public void MapEndpoint(IEndpointRouteBuilder app)
	{
		app.MapDelete("role/{userId}/{roleName}", async Task<EndpointResult<bool>> (
			[FromRoute] Guid userId,
			[FromRoute] string roleName,
			[FromServices] RemoveRoleHandler handler,
			CancellationToken token) => await handler.Handle(userId, roleName, token));
	}
}

public sealed class RemoveRoleHandler : IUserServiceHandler
{
	private readonly UserManager<UserDS> _userManager;
	private readonly RoleManager<RoleDS> _roleManager;
	private readonly ILogger<RemoveRoleHandler> _logger;
	private readonly IUserRepository _userRepository;

	public RemoveRoleHandler(
		UserManager<UserDS> userManager,
		RoleManager<RoleDS> roleManager,
		ILogger<RemoveRoleHandler> logger,
		IUserRepository userRepository)
	{
		_userManager = userManager;
		_roleManager = roleManager;
		_logger = logger;
		_userRepository = userRepository;
	}


	public async Task<Result<bool, Error>> Handle(Guid userId, string roleName, CancellationToken token)
	{
		var userResult = await _userRepository.GetByIdAsync(userId, token);
		if (userResult.IsFailure)
			return userResult.Error;

		var role = await _roleManager.FindByNameAsync(roleName);
		if (role == null)
		{
			return UserServiceErrors.RoleNotFound;
		}

		var result = await _userManager.RemoveFromRoleAsync(userResult.Value, roleName);
		if (!result.Succeeded)
		{
			var errors = string.Join("; ", result.Errors.Select(e => e.Description));
			return Error.Failure("role.removal_failed", errors);
		}

		_logger.LogInformation("Role {RoleName} removed from user {UserId}", roleName, userId);

		return true;
	}
}