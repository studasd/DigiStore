using CSharpFunctionalExtensions;
using DigiStore.Enums;
using DigiStore.Framework.Endpoints;
using DigiStore.SharedKernel;
using DigiStore.UserService.Application.Interfaces;
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

public sealed class UpdateLanguageHandler : IUserServiceHandler
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
		var userResult = await _userRepository.GetByIdAsync(userId, token);
		if (userResult.IsFailure)
			return userResult.Error;

		var user = userResult.Value;
		user.LangCode = langCode;
		user.UpdatedAt = DateTime.UtcNow;

		var updateResult = await _userRepository.UpdateAsync(user, token);
		if (updateResult.IsFailure)
			return updateResult.Error;

		return true;
	}
}