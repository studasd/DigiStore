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
		app.MapGet("getUser/byId/{userId}", async Task<EndpointResult<UserResponse>> (
			[FromRoute] Guid userId,
			[FromServices] GetUserByIdHandler handler,
			CancellationToken token) => await handler.Handle(userId, token));
	}
}

public sealed class GetUserByIdHandler : IUserServiceHandler
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
		// Get from database
		var userResult = await _userRepository.GetByIdAsync(userId, token);
		if (userResult.IsFailure)
			return userResult.Error;

		if (userResult.Value.IsDeleted)
		{
			_logger.LogWarning("User deleted: {UserId}", userId);
			return UserServiceErrors.UserNotFound;
		}

		// Get user roles
		var roles = await _userManager.GetRolesAsync(userResult.Value);

		return userResult.Value.ToUserResponse(roles);
	}
}