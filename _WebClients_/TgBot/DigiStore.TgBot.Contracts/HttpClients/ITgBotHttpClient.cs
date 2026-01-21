using CSharpFunctionalExtensions;
using DigiStore.SharedKernel;
using DigiStore.TgBot.Contracts.Requests;
using DigiStore.TgBot.Contracts.Responses;

namespace DigiStore.TgBot.Contracts.HttpClients;

public interface ITgBotHttpClient
{
	Task<Result<UpdatePaymentResponse, Error>> UpdatePaymentAsync(Guid userId, UpdatePaymentRequest request, CancellationToken token);
}