using CSharpFunctionalExtensions;
using DigiStore.Enums;
using DigiStore.SharedKernel;
using DigiStore.TgBot.Application.Constants;
using DigiStore.TgBot.Application.DTOs;
using DigiStore.TgBot.Application.Interfaces.Services;
using DigiStore.UserService.Contracts.HttpClients;
using DigiStore.UserService.Contracts.Requests;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace DigiStore.TgBot.Infrastructure.Postgres.Services;


public class TgUserService : ITgUserService
{
	private readonly IUserHttpClient _userClient;
	private readonly ILogger<TgUserService> _logger;

	public TgUserService(
		IUserHttpClient userClient,
		IConfiguration configuration,
		ILogger<TgUserService> logger)
	{
		_userClient = userClient;
		_logger = logger;
	}


	public async Task<Result<TgUserDto, Error>> GetOrCreateUserAsync(
		long telegramId,
		string? username,
		string? firstName,
		string? lastName,
		LanguageCodes langCode,
		CancellationToken token)
	{
		var responseResult = await _userClient.GetUserByTelegramId(telegramId, token);

		if (responseResult.IsFailure && responseResult.Error.Type == ErrorType.NOT_FOUND)
		{
			var createRequest = new CreateUserRequest
			{
				Email = $"telegram_{telegramId}@digistore.local",
				FirstName = firstName ?? string.Empty,
				LastName = lastName ?? string.Empty,
				TelegramId = telegramId,
				LangCode = langCode,
				Source = "Telegram"
			};

			var createResponse = await _userClient.RegisterUser(createRequest, token);

			if (createResponse.IsFailure)
			{
				_logger.LogWarning("Failed to create user in UserService for Telegram ID {TelegramId}: {Error}", telegramId, createResponse.Error?.GetMessage());
				return createResponse.Error ?? TgBotErrors.OperationFailed;
			}

			var createUser = createResponse.Value!;
			var newUser = new TgUserDto
			{
				Email = createUser.Email ?? string.Empty,
				FullName = createUser.FullName ?? string.Empty,
				Id = createUser.Id,
				TelegramId = createUser.TelegramId ?? telegramId,
				Username = username,
				IsActive = createUser.IsActive,
				LangCode = createUser.LangCode,
				Roles = createUser.Roles?.ToList() ?? new List<string>(),
				IsNew = true
			};

			_logger.LogInformation("User created for Telegram ID: {TelegramId}", telegramId);
			return newUser;
		}
		else if (responseResult.IsFailure)
		{
			_logger.LogWarning("GetUserByTelegramId failed for {TelegramId}: {Error}", telegramId, responseResult.Error?.GetMessage());
			return responseResult.Error ?? TgBotErrors.UserNotFound;
		}


		var user = responseResult.Value!;
		var telegramUser = new TgUserDto
		{
			Id = user.Id,
			TelegramId = user.TelegramId ?? telegramId,
			Email = user.Email ?? string.Empty,
			FullName = user.FullName ?? string.Empty,
			Username = username,
			LangCode = user.LangCode,
			IsActive = user.IsActive,
			Roles = user.Roles?.ToList() ?? new List<string>()
		};
		return telegramUser;
	}


	public async Task<Result<TgUserDto, Error>> GetUserProfileAsync(Guid userId, CancellationToken token)
	{
		var responseResult = await _userClient.GetUserById(userId, token);

		if (responseResult.IsFailure)
		{
			_logger.LogWarning("Failed to get user profile for user ID: {UserId}: {Error}", userId, responseResult.Error?.GetMessage());
			return responseResult.Error ?? TgBotErrors.UserNotFound;
		}

		var user = responseResult.Value!;
		var userDto = new TgUserDto
		{
			Id = user.Id,
			TelegramId = user.TelegramId ?? 0,
			Email = user.Email ?? string.Empty,
			FullName = user.FullName ?? string.Empty,
			// The UserResponse currently does not contain a Telegram username property. Do NOT use TelegramId
			// as Username. Leave Username null here; Profile caching will prefer the Telegram-side username when
			// available during GetOrCreateUserAsync.
			Username = null,
			LangCode = user.LangCode,
			IsActive = user.IsActive,
			Roles = user.Roles?.ToList() ?? new List<string>()
		};

		return userDto;
	}


	public async Task<UnitResult<Error>> UpdateLanguageAsync(Guid userId, LanguageCodes langCode, CancellationToken token)
	{
		var responseResult = await _userClient.UpdateLanguage(userId, langCode, token);

		if (responseResult.IsFailure)
		{
			_logger.LogWarning("Failed to update language for user ID: {UserId}", userId);
			return responseResult.Error;
		}

		_logger.LogInformation("Language updated for user ID: {UserId} to {LanguageCode}", userId, langCode);

		return Result.Success<Error>();
	}


	public async Task<UnitResult<Error>> UpdateActivityAsync(Guid userId, CancellationToken token)
	{
		var responseResult = await _userClient.UpdateActivity(userId, token);

		if (responseResult.IsFailure)
		{
			_logger.LogWarning("Failed to update activity for user ID: {UserId}", userId);
			return responseResult.Error;
		}

		return Result.Success<Error>();
	}
}
