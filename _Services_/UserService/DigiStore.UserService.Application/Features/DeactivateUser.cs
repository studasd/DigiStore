using CSharpFunctionalExtensions;
using DigiStore.Framework.Endpoints;
using DigiStore.SharedKernel;
using DigiStore.UserService.Application.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;

namespace DigiStore.UserService.Application.Features;

public sealed class DeactivateUser : IEndpoint
{
	public void MapEndpoint(IEndpointRouteBuilder app)
	{
		app.MapPost("deactivate/{userId}", async Task<EndpointResult<bool>> (
			[FromRoute] Guid userId,
			[FromServices] DeactivateUserHandler handler,
			CancellationToken token) => await handler.Handle(userId, token));
	}
}

public sealed class DeactivateUserHandler
{
	private readonly ILogger<DeactivateUserHandler> _logger;
	private readonly IUserRepository _userRepository;

	public DeactivateUserHandler(
		ILogger<DeactivateUserHandler> logger,
		IUserRepository userRepository)
	{
		_logger = logger;
		_userRepository = userRepository;
	}


	public async Task<Result<bool, Error>> Handle(Guid userId, CancellationToken token)
	{
		try
		{
			var user = await _userRepository.GetByIdAsync(userId, token);
			if (user == null)
			{
				return UserServiceErrors.UserNotFound;
			}

			user.IsActive = false;
			user.IsDeleted = false; // Can be reactivated
			user.UpdatedAt = DateTime.UtcNow;

			await _userRepository.UpdateAsync(user, token);
			_logger.LogInformation("User deactivated: {UserId}", userId);

			return true;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error deactivating user: {UserId}", userId);
			return Error.Failure("user.deactivation_error", ex.Message);
		}
	}
}