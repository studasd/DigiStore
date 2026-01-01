using CSharpFunctionalExtensions;
using DigiStore.Core.Validation;
using DigiStore.Framework.Endpoints;
using DigiStore.SharedKernel;
using DigiStore.SharedKernel.Extensions;
using DigiStore.UserService.Application.Interfaces;
using DigiStore.UserService.Contracts.Requests;
using DigiStore.UserService.Contracts.Responses;
using DigiStore.UserService.Domain;
using DigiStore.UserService.Domain.Enums;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;

namespace DigiStore.UserService.Application.Features;

public class CreateUserRequestValidator : AbstractValidator<CreateUserRequest>
{
	public CreateUserRequestValidator()
	{
	}
}


public sealed class RegisterUser : IEndpoint
{
	public void MapEndpoint(IEndpointRouteBuilder app)
	{
		app.MapPost("/register", async Task<EndpointResult<UserResponse>> (
			[FromBody] CreateUserRequest request,
			[FromServices] RegisterUserHandler handler,
			CancellationToken token) => await handler.Handle(request, token));
	}
}

public sealed class RegisterUserHandler
{
	private readonly UserManager<UserDS> _userManager;
	private readonly ILogger<RegisterUserHandler> _logger;
	private readonly IUserRepository _userRepository;
	private readonly IValidator<CreateUserRequest> _validator;

	public RegisterUserHandler(
		UserManager<UserDS> userManager,
		ILogger<RegisterUserHandler> logger,
		IUserRepository userRepository,
		IValidator<CreateUserRequest> validator)
	{
		_userManager = userManager;
		_logger = logger;
		_userRepository = userRepository;
		_validator = validator;
	}


	public async Task<Result<UserResponse, Error>> Handle(CreateUserRequest request, CancellationToken token)
	{
		ValidationResult validationResult = await _validator.ValidateAsync(request, token);
		if (!validationResult.IsValid)
		{
			return validationResult.ToError();
		}

		try
		{
			// Check if email already exists
			if (await _userRepository.ExistsByEmailAsync(request.Email, token))
			{
				_logger.LogWarning("Attempt to create user with existing email: {Email}", request.Email);
				return UserServiceErrors.UserAlreadyExists;
			}

			// Check if Telegram ID already linked
			if (request.TelegramId.HasValue &&
				await _userRepository.ExistsByTelegramIdAsync(request.TelegramId.Value, token))
			{
				_logger.LogWarning("Attempt to link already linked Telegram ID: {TelegramId}", request.TelegramId);
				return UserServiceErrors.TelegramIdAlreadyLinked;
			}

			var userSourceParse = request.Source.ParseEnum<UserSource>();

			if(userSourceParse.IsFailure)
				return userSourceParse.Error;


			// Create user entity
			var user = new UserDS
			{
				Id = Guid.NewGuid(),
				Email = request.Email,
				UserName = request.Email,
				FirstName = request.FirstName ?? string.Empty,
				LastName = request.LastName ?? string.Empty,
				TelegramId = request.TelegramId,
				PhoneNumber = request.PhoneNumber,
				LanguageCode = request.LanguageCode,
				Source = userSourceParse.Value,
				IsActive = true,
				CreatedAt = DateTime.UtcNow,
				UpdatedAt = DateTime.UtcNow,
			};

			// Create user with password if provided (Web users)
			IdentityResult result;
			if (!string.IsNullOrEmpty(request.Password))
			{
				result = await _userManager.CreateAsync(user, request.Password);
			}
			else
			{
				// For Telegram users without password
				result = await _userManager.CreateAsync(user);
			}

			if (!result.Succeeded)
			{
				var errors = string.Join("; ", result.Errors.Select(e => e.Description));
				_logger.LogError("Failed to create user: {Errors}", errors);
				return Error.Failure("user.creation_failed", errors);
			}

			// Assign default role
			await _userManager.AddToRoleAsync(user, "User");

			_logger.LogInformation("User created successfully: {UserId}", user.Id);

			// Cache the new user
			var response = new UserResponse
			(
				user.Id,
				user.Email,
				user.FirstName,
				user.TelegramId,
				user.PhoneNumber,
				user.LanguageCode,
				true,
				user.Source.ToString(),
				new List<string> { "User" },
				user.CreatedAt,
				user.UpdatedAt
			);

			return response;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error creating user");
			return Error.Failure("user.creation_error", ex.Message);
		}
	}
}