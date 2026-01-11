using CSharpFunctionalExtensions;
using DigiStore.SharedKernel;
using DigiStore.TgBot.Application.Constants;
using DigiStore.TgBot.Application.DTOs;
using DigiStore.TgBot.Application.Interfaces.Services;
using DigiStore.UserService.Contracts.HttpClients;
using DigiStore.UserService.Contracts.Requests;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace DigiStore.TgBot.Application.Services;


public class UserService : IUserService
{
	private readonly IUserHttpClient _httpClient;
	//private readonly HttpClient _httpClient;
	private readonly ILogger<UserService> _logger;

	public UserService(
		//HttpClient httpClient,
		IUserHttpClient httpClient,
		IConfiguration configuration,
		ILogger<UserService> logger)
	{
		_httpClient = httpClient;
		//_httpClient = httpClient;
		_logger = logger;
	}


	public async Task<Result<TelegramUserDto, Error>> GetOrCreateUserAsync(
		long telegramId,
		string? username,
		string? firstName,
		string? lastName,
		string languageCode,
		CancellationToken ct = default)
	{
		try
		{
			var responseResult = await _httpClient.GetUserByTelegramId(telegramId, ct);


			if (responseResult.IsSuccess)
			{
				var user = responseResult.Value!;
				var telegramUser = new TelegramUserDto
				{
					Id = user.Id,
					TelegramId = user.TelegramId ?? telegramId,
					Email = user.Email ?? string.Empty,
					FullName = user.FullName ?? string.Empty,
					Username = user.TelegramId.HasValue ? user.TelegramId.ToString() : username,
					LangCode = user.LanguageCode,
					IsActive = user.IsActive,
					Roles = user.Roles?.ToList() ?? new List<string>()
				};
				return telegramUser;
			}
			else if (responseResult.IsFailure && responseResult.Error.Type == ErrorType.NOT_FOUND)
			{
				var createRequest = new CreateUserRequest
				{
					Email = $"telegram_{telegramId}@digistore.local",
					FirstName = firstName ?? string.Empty,
					LastName = lastName ?? string.Empty,
					TelegramId = telegramId,
					TelegramUsername = username,
					LanguageCode = languageCode,
					Source = "Telegram"
				};

				var createResponse = await _httpClient.RegisterUser(createRequest, ct);

				if (createResponse.IsSuccess)
				{
					var createUser = createResponse.Value!;
					var newUser = new TelegramUserDto
					{
						Email = createUser.Email ?? string.Empty,
						FullName = createUser.FullName ?? string.Empty,
						Id = createUser.Id,
						TelegramId = createUser.TelegramId ?? telegramId,
						Username = username,
						IsActive = createUser.IsActive,
						LangCode = createUser.LanguageCode,
						Roles = createUser.Roles?.ToList() ?? new List<string>(),
						IsNew = true
					};

					_logger.LogInformation("User created for Telegram ID: {TelegramId}", telegramId);
					return newUser;
				}
				else
				{
					_logger.LogWarning("Failed to create user in UserService for Telegram ID {TelegramId}: {Error}", telegramId, createResponse.Error?.GetMessage());
					return createResponse.Error ?? TgBotErrors.OperationFailed;
				}
			}
			else if (responseResult.IsFailure)
			{
				_logger.LogWarning("GetUserByTelegramId failed for {TelegramId}: {Error}", telegramId, responseResult.Error?.GetMessage());
				return responseResult.Error ?? TgBotErrors.UserNotFound;
			}

			_logger.LogError("Failed to get or create user for Telegram ID: {TelegramId}", telegramId);
			return TgBotErrors.UserNotFound;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error in GetOrCreateUserAsync for Telegram ID: {TelegramId}", telegramId);
			return Error.Failure("bot.user_service_error", ex.Message);
		}
	}


	public async Task<Result<TelegramUserDto, Error>> GetUserProfileAsync(Guid userId, CancellationToken ct = default)
	{
		try
		{
			var response = await _httpClient.GetUserById(userId, ct);

			if (response.IsFailure)
			{
				_logger.LogWarning("Failed to get user profile for user ID: {UserId}: {Error}", userId, response.Error?.GetMessage());
				return response.Error ?? TgBotErrors.UserNotFound;
			}

			var user = response.Value!;
			var dto = new TelegramUserDto
			{
				Id = user.Id,
				TelegramId = user.TelegramId ?? 0,
				Email = user.Email ?? string.Empty,
				FullName = user.FullName ?? string.Empty,
				Username = user.TelegramId.HasValue ? user.TelegramId.ToString() : null,
				LangCode = user.LanguageCode,
				IsActive = user.IsActive,
				Roles = user.Roles?.ToList() ?? new List<string>()
			};

			return dto;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error getting user profile for user ID: {UserId}", userId);
			return Error.Failure("bot.user_service_error", ex.Message);
		}
	}


	public async Task<Result<bool, Error>> UpdateLanguageAsync(Guid userId, string languageCode, CancellationToken ct = default)
	{
		try
		{
			var response = await _httpClient.UpdateLanguage(userId, languageCode, ct);

			if (response.IsFailure)
			{
				_logger.LogWarning("Failed to update language for user ID: {UserId}", userId);
				return TgBotErrors.OperationFailed;
			}

			_logger.LogInformation("Language updated for user ID: {UserId} to {LanguageCode}", userId, languageCode);
			return true;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error updating language for user ID: {UserId}", userId);
			return Error.Failure("bot.user_service_error", ex.Message);
		}
	}


	public async Task<Result<bool, Error>> UpdateActivityAsync(Guid userId, CancellationToken ct = default)
	{
		try
		{
			var response = await _httpClient.UpdateActivity(userId, ct);

			if (response.IsFailure)
			{
				_logger.LogWarning("Failed to update activity for user ID: {UserId}", userId);
				return TgBotErrors.OperationFailed;
			}

			return true;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error updating activity for user ID: {UserId}", userId);
			return Error.Failure("bot.user_service_error", ex.Message);
		}
	}
}
