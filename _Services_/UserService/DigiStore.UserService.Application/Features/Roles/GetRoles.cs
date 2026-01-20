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

namespace DigiStore.UserService.Application.Features.Roles;


public sealed class GetRoles : IEndpoint
{
	public void MapEndpoint(IEndpointRouteBuilder app)
	{
		app.MapGet("roles/{userId}", async Task<EndpointResult<IReadOnlyList<string>>> (
			[FromRoute] Guid userId,
			[FromServices] GetRolesHandler handler,
			CancellationToken token) => await handler.Handle(userId, token));
	}
}


public sealed class GetRolesHandler : IUserServiceHandler
{
	private readonly UserManager<UserDS> _userManager;
	private readonly ILogger<GetRolesHandler> _logger;
	private readonly IUserRepository _userRepository;

	public GetRolesHandler(
		UserManager<UserDS> userManager,
		ILogger<GetRolesHandler> logger,
		IUserRepository userRepository)
	{
		_userManager = userManager;
		_logger = logger;
		_userRepository = userRepository;
	}


	public async Task<Result<IReadOnlyList<string>, Error>> Handle(Guid userId, CancellationToken token)
	{
		var userResult = await _userRepository.GetByIdAsync(userId, token);
		if (userResult.IsFailure)
			return userResult.Error;

		var roles = (await _userManager.GetRolesAsync(userResult.Value)).ToList();

		return roles;
	}
}