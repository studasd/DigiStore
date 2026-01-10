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


public class TelegramUserService : ITelegramUserService
{
	private readonly IUserHttpClient _httpClient;
	//private readonly HttpClient _httpClient;
	private readonly ILogger<TelegramUserService> _logger;
	private readonly string _userServiceUrl;

	public TelegramUserService(
		//HttpClient httpClient,
		IUserHttpClient httpClient,
		IConfiguration configuration,
		ILogger<TelegramUserService> logger)
	{
		_httpClient = httpClient;
		//_httpClient = httpClient;
		_logger = logger;
		_userServiceUrl = configuration["Services:UserService:Url"]
			?? throw new InvalidOperationException("UserService URL not configured");
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
			//// First, try to get existing user
			////var getUrl = $"{_userServiceUrl}/api/account/by-telegram/{telegramId}";
			//var getUrl = $"{_userServiceUrl}/getUser/byTelegram/{telegramId}";
			//var response = await _httpClient.GetAsync(getUrl, ct);
			var responseResult = await _httpClient.GetUserByTelegramId(telegramId, ct);


			if(responseResult.IsSuccess)
			{
				var user = responseResult.Value;
				var telegramUser = new TelegramUserDto
				{
					Id = user.Id,
					TelegramId = user.TelegramId.Value,
					Email = user.Email,
					FullName = user.FullName,
					TelegramUsername = username,
					LanguageCode = user.LanguageCode,
					IsActive = user.IsActive,
					Roles = user.Roles.Select(r => r).ToList()
				};
				return telegramUser;
			}
			else if (responseResult.IsFailure && responseResult.Error.Type == ErrorType.NOT_FOUND)
			{
				// If user doesn't exist, create new one
				var createUrl = $"{_userServiceUrl}/register";
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
					//var responseContent = await createResponse.Content.ReadAsStringAsync(ct);
					//var newUser = JsonSerializer.Deserialize<TelegramUserDto>(responseContent);
					var createUser = createResponse.Value;
					var newUser = new TelegramUserDto
					{
						Email = createUser.Email,
						FullName = createUser.FullName,
						Id = createUser.Id,
						TelegramId = createUser.TelegramId.Value,
						TelegramUsername = username,
						IsActive = createUser.IsActive,
						LanguageCode = createUser.LanguageCode,
						Roles = createUser.Roles.Select(r => r).ToList()
					};

					_logger.LogInformation("User created for Telegram ID: {TelegramId}", telegramId);
					return newUser;
				}
			}
			else if (responseResult.IsFailure)
			{
				//var content = await responseResult.Content.ReadAsStringAsync(ct);
				//var user = JsonSerializer.Deserialize<TelegramUserDto>(content);
				return responseResult.Error;
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
				_logger.LogWarning("Failed to get user profile for user ID: {UserId}", userId);
				return response.Error;
			}

			//////var content = await response.Content.ReadAsStringAsync(ct);
			//////var user = JsonSerializer.Deserialize<TelegramUserDto>(content);

			return new TelegramUserDto();
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
