using CSharpFunctionalExtensions;
using DigiStore.TgBot.Contracts.Requests;
using StudCoreKit.SharedKernel;

namespace DigiStore.TgBot.Contracts.HttpClients;

public interface ITgBotHttpClient
{
	Task<UnitResult<Error>> UpdatePaymentAsync(Guid userId, UpdatePaymentRequest request, CancellationToken token);
	Task<UnitResult<Error>> CancelPaymentAsync(Guid userId, CancelPaymentRequest request, CancellationToken token);
}