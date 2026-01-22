using CSharpFunctionalExtensions;
using DigiStore.SharedKernel;
using DigiStore.TgBot.Contracts.Requests;

namespace DigiStore.TgBot.Contracts.HttpClients;

public interface ITgBotHttpClient
{
	Task<UnitResult<Error>> UpdatePaymentAsync(Guid userId, UpdatePaymentRequest request, CancellationToken token);
}