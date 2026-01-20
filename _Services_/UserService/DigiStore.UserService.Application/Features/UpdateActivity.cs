using CSharpFunctionalExtensions;
using DigiStore.Framework.Endpoints;
using DigiStore.SharedKernel;
using DigiStore.UserService.Application.Interfaces;
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


public sealed class UpdateActivity : IEndpoint
{
	public void MapEndpoint(IEndpointRouteBuilder app)
	{
		app.MapPost("activity/{userId}", async Task<EndpointResult<bool>> (
			[FromRoute] Guid userId,
			[FromServices] UpdateActivityHandler handler,
			CancellationToken token) => await handler.Handle(userId, token));
	}
}

public sealed class UpdateActivityHandler : IUserServiceHandler
{
	private readonly UserManager<UserDS> _userManager;
	private readonly ILogger<UpdateActivityHandler> _logger;
	private readonly IUserRepository _userRepository;

	public UpdateActivityHandler(
		UserManager<UserDS> userManager,
		ILogger<UpdateActivityHandler> logger,
		IUserRepository userRepository)
	{
		_userManager = userManager;
		_logger = logger;
		_userRepository = userRepository;
	}


	public async Task<Result<bool, Error>> Handle(Guid userId, CancellationToken token)
	{
		var userResult = await _userRepository.GetByIdAsync(userId, token);
		if (userResult.IsFailure)
			return userResult.Error;

		var user = userResult.Value;
		user.LastActivityAt = DateTime.UtcNow;

		var updateResult = await _userRepository.UpdateAsync(user, token);
		if (updateResult.IsFailure)
			return updateResult.Error;

		return true;
	}
}