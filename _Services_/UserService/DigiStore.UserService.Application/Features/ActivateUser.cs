using CSharpFunctionalExtensions;
using DigiStore.Framework.Endpoints;
using DigiStore.SharedKernel;
using DigiStore.UserService.Application.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;

namespace DigiStore.UserService.Application.Features;

public sealed class ActivateUser : IEndpoint
{
	public void MapEndpoint(IEndpointRouteBuilder app)
	{
		app.MapPost("activate/{userId}", async Task<EndpointResult<bool>> (
			[FromRoute] Guid userId,
			[FromServices] ActivateUserHandler handler,
			CancellationToken token) => await handler.Handle(userId, token));
	}
}

public sealed class ActivateUserHandler : IUserServiceHandler
{
	private readonly ILogger<ActivateUserHandler> _logger;
	private readonly IUserRepository _userRepository;

	public ActivateUserHandler(
		ILogger<ActivateUserHandler> logger,
		IUserRepository userRepository)
	{
		_logger = logger;
		_userRepository = userRepository;
	}


	public async Task<Result<bool, Error>> Handle(Guid userId, CancellationToken token)
	{
		var userResult = await _userRepository.GetByIdAsync(userId, token);
		if (userResult.IsFailure)
			return userResult.Error;

		var user = userResult.Value;
		user.IsActive = true;
		user.UpdatedAt = DateTime.UtcNow;

		var updateResult = await _userRepository.UpdateAsync(user, token);
		if(updateResult.IsFailure)
			return updateResult.Error;

		_logger.LogInformation("User activated: {UserId}", userId);

		return true;
	}
}