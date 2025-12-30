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



public sealed class GetUserById : IEndpoint
{
	public void MapEndpoint(IEndpointRouteBuilder app)
	{
		app.MapGet("getUser/{userId:guid}", async Task<EndpointResult<UserResponse>> (
			[FromRoute] Guid userId,
			[FromServices] GetUserByIdHandler handler,
			CancellationToken token) => await handler.Handle(userId, token));
	}
}

public sealed class GetUserByIdHandler
{
	private readonly UserManager<UserDS> _userManager;
	private readonly ILogger<GetUserByIdHandler> _logger;
	private readonly IUserRepository _userRepository;

	public GetUserByIdHandler(
		UserManager<UserDS> userManager,
		ILogger<GetUserByIdHandler> logger,
		IUserRepository userRepository)
	{
		_userManager = userManager;
		_logger = logger;
		_userRepository = userRepository;
	}


	public async Task<Result<UserResponse, Error>> Handle(Guid userId, CancellationToken token)
	{
		try
		{
			// Get from database
			var user = await _userRepository.GetByIdAsync(userId, token);
			if (user == null || user.IsDeleted)
			{
				_logger.LogWarning("User not found: {UserId}", userId);
				return UserServiceErrors.UserNotFound;
			}

			// Get user roles
			var roles = await _userManager.GetRolesAsync(user);

			return user.ToUserResponse(roles);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error getting user by ID: {UserId}", userId);
			return Error.Failure("user.retrieval_error", ex.Message);
		}
	}
}