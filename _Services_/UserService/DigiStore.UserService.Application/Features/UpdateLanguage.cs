using CSharpFunctionalExtensions;
using DigiStore.Framework.Endpoints;
using DigiStore.SharedKernel;
using DigiStore.UserService.Application.Interfaces;
using DigiStore.UserService.Contracts.Enums;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace DigiStore.UserService.Application.Features;

public sealed class UpdateLanguage : IEndpoint
{
	public void MapEndpoint(IEndpointRouteBuilder app)
	{
		app.MapPost("language/{userId}/{langCode}", async Task<EndpointResult<bool>> (
			[FromRoute] Guid userId,
			[FromRoute] LanguageCodes langCode,
			[FromServices] UpdateLanguageHandler handler,
			CancellationToken token) => await handler.Handle(userId, langCode, token));
	}
}

public sealed class UpdateLanguageHandler
{
	private readonly ILogger<UpdateLanguageHandler> _logger;
	private readonly IUserRepository _userRepository;

	public UpdateLanguageHandler(
		ILogger<UpdateLanguageHandler> logger,
		IUserRepository userRepository)
	{
		_logger = logger;
		_userRepository = userRepository;
	}


	public async Task<Result<bool, Error>> Handle(Guid userId, LanguageCodes langCode, CancellationToken token)
	{
		try
		{
			var user = await _userRepository.GetByIdAsync(userId, token);
			if (user == null)
			{
				return UserServiceErrors.UserNotFound;
			}

			user.LangCode = langCode;
			user.UpdatedAt = DateTime.UtcNow;
			await _userRepository.UpdateAsync(user, token);

			return true;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error updating language: {UserId}", userId);
			return Error.Failure("user.language_update_error", ex.Message);
		}
	}
}