using CSharpFunctionalExtensions;
using DigiStore.Enums;
using DigiStore.SharedKernel;
using DigiStore.SharedKernel.HttpServices;
using DigiStore.UserService.Contracts.Requests;
using DigiStore.UserService.Contracts.Responses;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace DigiStore.UserService.Contracts.HttpClients;

internal sealed class UserHttpClient : IUserHttpClient
{
	private readonly ILogger<UserHttpClient> _logger;
    private readonly HttpService _httpService;

    public UserHttpClient(ILogger<UserHttpClient> logger, IHttpServiceFactory httpServiceFactory)
	{
		_logger = logger;
        _httpService = httpServiceFactory.CreateHttpService<UserHttpClient>();
    }


	public async Task<Result<UserResponse, Error>> GetUserByTelegramId(long telegramId, CancellationToken token)
	{
		return await _httpService.GetAsync<UserResponse>($"/getUser/byTelegram/{telegramId}", token);
	}
	
	public async Task<Result<UserResponse, Error>> GetUserById(Guid userId, CancellationToken token)
	{
		return await _httpService.GetAsync<UserResponse>($"/getUser/byId/{userId}", token);
	}
	
	
	public async Task<UnitResult<Error>> UpdateLanguage(Guid userId, LanguageCodes langCode, CancellationToken token)
	{
		return await _httpService.PostAsync($"/language/{userId}/{langCode}", null, token);
	}


	public async Task<UnitResult<Error>> UpdateActivity(Guid userId, CancellationToken token)
	{
		return await _httpService.PostAsync($"/activity/{userId}", null, token);
	}


	public async Task<Result<UserResponse, Error>> RegisterUser(CreateUserRequest request, CancellationToken token)
	{
		
		var json = JsonSerializer.Serialize(request);
		var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

		return await _httpService.PostAsync<UserResponse>($"/register", content, token);
	}
}