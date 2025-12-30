using CSharpFunctionalExtensions;
using DigiStore.Framework.Endpoints;
using DigiStore.SharedKernel;
using DigiStore.UserService.Application.Interfaces;
using DigiStore.UserService.Application.Mappers;
using DigiStore.UserService.Contracts.Responses;
using DigiStore.UserService.Domain;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace DigiStore.UserService.Application.Features;

public sealed class GetUserByEmail : IEndpoint
{
	public void MapEndpoint(IEndpointRouteBuilder app)
	{
		app.MapGet("getUser/{email:string}", async Task<EndpointResult<UserResponse>> (
			[FromRoute] string email,
			[FromServices] GetUserByEmailHandler handler,
			CancellationToken token) => await handler.Handle(email, token));
	}
}

public sealed class GetUserByEmailHandler
{
	private readonly UserManager<UserDS> _userManager;
	private readonly ILogger<GetUserByEmailHandler> _logger;
	private readonly IUserRepository _userRepository;

	public GetUserByEmailHandler(
		UserManager<UserDS> userManager,
		ILogger<GetUserByEmailHandler> logger,
		IUserRepository userRepository)
	{
		_userManager = userManager;
		_logger = logger;
		_userRepository = userRepository;
	}


	public async Task<Result<UserResponse, Error>> Handle(string email, CancellationToken token)
	{
		try
		{
			var user = await _userRepository.GetByEmailAsync(email, token);
			if (user == null || user.IsDeleted)
			{
				_logger.LogWarning("User not found by email: {Email}", email);
				return UserServiceErrors.UserNotFound;
			}

			var roles = await _userManager.GetRolesAsync(user);
			
			return user.ToUserResponse(roles);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error getting user by email: {Email}", email);
			return Error.Failure("user.retrieval_error", ex.Message);
		}
	}
}