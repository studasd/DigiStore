using CSharpFunctionalExtensions;
using DigiStore.Enums;
using DigiStore.SharedKernel;
using DigiStore.UserService.Contracts.Requests;
using DigiStore.UserService.Contracts.Responses;

namespace DigiStore.UserService.Contracts.HttpClients;

public interface IUserHttpClient
{
	Task<Result<UserResponse, Error>> GetUserById(Guid userId, CancellationToken token);
	Task<Result<UserResponse, Error>> GetUserByTelegramId(long telegramId, CancellationToken token);
	
	Task<UnitResult<Error>> UpdateLanguage(Guid userId, LanguageCodes langCode, CancellationToken token);
	Task<UnitResult<Error>> UpdateActivity(Guid userId, CancellationToken token);

	Task<Result<UserResponse, Error>> RegisterUser(CreateUserRequest request, CancellationToken token);
}