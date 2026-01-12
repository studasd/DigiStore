using CSharpFunctionalExtensions;
using DigiStore.SharedKernel;
using DigiStore.UserService.Contracts.Enums;
using DigiStore.UserService.Contracts.Requests;
using DigiStore.UserService.Contracts.Responses;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace DigiStore.UserService.Contracts.HttpClients;

internal sealed class UserHttpClient : IUserHttpClient
{
	private readonly HttpClient _httpClient;
	private readonly ILogger<UserHttpClient> _logger;

	public UserHttpClient(HttpClient httpClient, ILogger<UserHttpClient> logger)
	{
		_httpClient = httpClient;
		_logger = logger;
	}


	public async Task<Result<UserResponse, Error>> GetUserByTelegramId(long telegramId, CancellationToken cancellationToken)
	{
		try
		{
			HttpResponseMessage response = await _httpClient.GetAsync($"/getUser/byTelegram/{telegramId}", cancellationToken);
			return await response.HandleResponseAsync<UserResponse>(cancellationToken);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error get user by telegram id for {telegramId}", telegramId);

			return Error.Failure("server.internal", "Failed to request get user by telegram id");
		}
	}
	
	public async Task<Result<UserResponse, Error>> GetUserById(Guid userId, CancellationToken cancellationToken)
	{
		try
		{
			HttpResponseMessage response = await _httpClient.GetAsync($"/getUser/byId/{userId}", cancellationToken);
			return await response.HandleResponseAsync<UserResponse>(cancellationToken);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error get user by id for {userId}", userId);

			return Error.Failure("server.internal", "Failed to request get user by id");
		}
	}
	
	
	public async Task<Result<bool, Error>> UpdateLanguage(Guid userId, LanguageCodes langCode, CancellationToken cancellationToken)
	{
		try
		{
			HttpResponseMessage response = await _httpClient.PostAsync($"/language/{userId}/{langCode}", null, cancellationToken);
			var result = await response.HandleResponseAsync(cancellationToken);

			if (result.IsSuccess)
			{
				return true;
			}

			return false;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error update language for {userId}", userId);

			return Error.Failure("server.internal", "Failed to request update language");
		}
	}
	
	
	public async Task<Result<bool, Error>> UpdateActivity(Guid userId, CancellationToken cancellationToken)
	{
		try
		{
			HttpResponseMessage response = await _httpClient.PostAsync($"/activity/{userId}", null, cancellationToken);
			var result = await response.HandleResponseAsync(cancellationToken);
			
			if (result.IsSuccess)
			{
				return true;
			}

			return false;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error getting media assets for {userId}", userId);

			return Error.Failure("server.internal", "Failed to request media assets info");
		}
	}
	
	
	public async Task<Result<UserResponse, Error>> RegisterUser(CreateUserRequest request, CancellationToken cancellationToken)
	{
		try
		{
			var json = JsonSerializer.Serialize(request);
			var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

			HttpResponseMessage response = await _httpClient.PostAsync($"/register", content, cancellationToken);
			return await response.HandleResponseAsync<UserResponse>(cancellationToken);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error register user for {telegramId}", request.TelegramId);

			return Error.Failure("server.internal", "Failed to request register user");
		}
	}
}