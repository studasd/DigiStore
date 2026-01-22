using CSharpFunctionalExtensions;
using DigiStore.SharedKernel;
using DigiStore.SharedKernel.HttpServices;
using DigiStore.TgBot.Contracts.Requests;
using Microsoft.Extensions.Logging;

namespace DigiStore.TgBot.Contracts.HttpClients;

internal sealed class TgBotHttpClient : ITgBotHttpClient
{
	private readonly ILogger<TgBotHttpClient> _logger;
    private readonly HttpService _httpService;

    public TgBotHttpClient(ILogger<TgBotHttpClient> logger, IHttpServiceFactory httpServiceFactory)
	{
		_logger = logger;
        _httpService = httpServiceFactory.CreateHttpService<TgBotHttpClient>();
    }


	public async Task<UnitResult<Error>> UpdatePaymentAsync(Guid userId, UpdatePaymentRequest request, CancellationToken token)
	{
		return await _httpService.PostAsync($"updatePayment/{userId}", request, token);
	}

	public async Task<UnitResult<Error>> CancelPaymentAsync(Guid userId, CancelPaymentRequest request, CancellationToken token)
	{
		return await _httpService.PostAsync($"cancelPayment/{userId}", request, token);
	}
}