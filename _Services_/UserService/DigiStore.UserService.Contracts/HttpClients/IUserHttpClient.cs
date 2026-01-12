using CSharpFunctionalExtensions;
using DigiStore.SharedKernel;
using DigiStore.UserService.Contracts.Enums;
using DigiStore.UserService.Contracts.Requests;
using DigiStore.UserService.Contracts.Responses;

namespace DigiStore.UserService.Contracts.HttpClients;

public interface IUserHttpClient
{
	Task<Result<UserResponse, Error>> GetUserById(Guid userId, CancellationToken cancellationToken);
	Task<Result<UserResponse, Error>> GetUserByTelegramId(long telegramId, CancellationToken cancellationToken);
	
	Task<Result<bool, Error>> UpdateLanguage(Guid userId, LanguageCodes langCode, CancellationToken cancellationToken);
	Task<Result<bool, Error>> UpdateActivity(Guid userId, CancellationToken cancellationToken);

	Task<Result<UserResponse, Error>> RegisterUser(CreateUserRequest request, CancellationToken cancellationToken);
}