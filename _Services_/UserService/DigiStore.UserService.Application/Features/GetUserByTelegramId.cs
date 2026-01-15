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

public sealed class GetUserByTelegramId : IEndpoint
{
	public void MapEndpoint(IEndpointRouteBuilder app)
	{
		app.MapGet("getUser/byTelegram/{telegramId}", async Task<EndpointResult<UserResponse>> (
			[FromRoute] long telegramId,
			[FromServices] GetUserByTelegramIdHandler handler,
			CancellationToken token) => await handler.Handle(telegramId, token));
	}
}

public sealed class GetUserByTelegramIdHandler : IUserServiceHandler
{
	private readonly UserManager<UserDS> _userManager;
	private readonly ILogger<GetUserByTelegramIdHandler> _logger;
	private readonly IUserRepository _userRepository;

	public GetUserByTelegramIdHandler(
		UserManager<UserDS> userManager,
		ILogger<GetUserByTelegramIdHandler> logger,
		IUserRepository userRepository)
	{
		_userManager = userManager;
		_logger = logger;
		_userRepository = userRepository;
	}


	public async Task<Result<UserResponse, Error>> Handle(long telegramId, CancellationToken token)
	{
		try
		{
			var user = await _userRepository.GetByTelegramIdAsync(telegramId, token);
			if (user == null || user.IsDeleted)
			{
				_logger.LogWarning("User not found by Telegram ID: {TelegramId}", telegramId);
				return UserServiceErrors.UserNotFound;
			}

			_logger.LogInformation("User by Telegram ID: {TelegramId}", telegramId);

			var roles = await _userManager.GetRolesAsync(user);

			return user.ToUserResponse(roles);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error getting user by Telegram ID: {TelegramId}", telegramId);
			return Error.Failure("user.retrieval_error", ex.Message);
		}
	}
}