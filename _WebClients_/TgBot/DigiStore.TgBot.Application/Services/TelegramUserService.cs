using CSharpFunctionalExtensions;
using DigiStore.SharedKernel;
using DigiStore.TgBot.Application.Constants;
using DigiStore.TgBot.Application.DTOs;
using DigiStore.TgBot.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace DigiStore.TgBot.Application.Services;


public class TelegramUserService : ITelegramUserService
{
	private readonly HttpClient _httpClient;
	private readonly ILogger<TelegramUserService> _logger;
	private readonly string _userServiceUrl;

	public TelegramUserService(
		HttpClient httpClient,
		IConfiguration configuration,
		ILogger<TelegramUserService> logger)
	{
		_httpClient = httpClient;
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
			// First, try to get existing user
			var getUrl = $"{_userServiceUrl}/api/account/by-telegram/{telegramId}";
			var response = await _httpClient.GetAsync(getUrl, ct);

			if (response.IsSuccessStatusCode)
			{
				var content = await response.Content.ReadAsStringAsync(ct);
				var user = JsonSerializer.Deserialize<TelegramUserDto>(content);
				return user;
			}

			// If user doesn't exist, create new one
			if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
			{
				var createUrl = $"{_userServiceUrl}/api/account/register";
				var createRequest = new
				{
					Email = $"telegram_{telegramId}@petfamily.local",
					FirstName = firstName ?? string.Empty,
					LastName = lastName ?? string.Empty,
					TelegramId = telegramId,
					TelegramUsername = username,
					LanguageCode = languageCode,
					Source = "Telegram"
				};

				var json = JsonSerializer.Serialize(createRequest);
				var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

				var createResponse = await _httpClient.PostAsync(createUrl, content, ct);

				if (createResponse.IsSuccessStatusCode)
				{
					var responseContent = await createResponse.Content.ReadAsStringAsync(ct);
					var newUser = JsonSerializer.Deserialize<TelegramUserDto>(responseContent);
					_logger.LogInformation("User created for Telegram ID: {TelegramId}", telegramId);
					return newUser;
				}
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
			var url = $"{_userServiceUrl}/api/account/{userId}";
			var response = await _httpClient.GetAsync(url, ct);

			if (!response.IsSuccessStatusCode)
			{
				_logger.LogWarning("Failed to get user profile for user ID: {UserId}", userId);
				return TgBotErrors.UserNotFound;
			}

			var content = await response.Content.ReadAsStringAsync(ct);
			var user = JsonSerializer.Deserialize<TelegramUserDto>(content);

			return user;
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
			var url = $"{_userServiceUrl}/api/account/{userId}/language?languageCode={languageCode}";
			var response = await _httpClient.PatchAsync(url, null, ct);

			if (!response.IsSuccessStatusCode)
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
			var url = $"{_userServiceUrl}/api/account/activity/{userId}";
			var response = await _httpClient.PostAsync(url, null, ct);

			if (!response.IsSuccessStatusCode)
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
