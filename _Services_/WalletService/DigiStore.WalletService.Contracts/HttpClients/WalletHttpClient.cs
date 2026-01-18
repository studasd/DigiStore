using CSharpFunctionalExtensions;
using DigiStore.SharedKernel;
using DigiStore.SharedKernel.HttpServices;
using DigiStore.WalletService.Contracts.Requests;
using DigiStore.WalletService.Contracts.Requests.Payments;
using DigiStore.WalletService.Contracts.Responses;
using DigiStore.WalletService.Contracts.Responses.Payments;
using Microsoft.Extensions.Logging;

namespace DigiStore.WalletService.Contracts.HttpClients;

internal sealed class WalletHttpClient : IWalletHttpClient
{
	private readonly ILogger<WalletHttpClient> _logger;
    private readonly HttpService _httpService;

    public WalletHttpClient(ILogger<WalletHttpClient> logger, IHttpServiceFactory httpServiceFactory)
	{
		_logger = logger;
        _httpService = httpServiceFactory.CreateHttpService<WalletHttpClient>();
    }


	public async Task<Result<CheckBalanceResponse, Error>> CheckBalanceAsync(Guid userId, decimal amount, CancellationToken token)
	{
		return await _httpService.PostAsync<CheckBalanceResponse>($"checkBalance/{userId}/{amount}", null, token);
	}
	
	public async Task<Result<TransactionResponse, Error>> DepositAsync(Guid userId, DepositRequest request, CancellationToken token)
	{
		return await _httpService.PostAsync<TransactionResponse>($"deposit/{userId}", request,  token);
	}
	
	
	public async Task<UnitResult<Error>> FreezeWalletAsync(Guid userId, CancellationToken token)
	{
		return await _httpService.PostAsync($"freezeWallet/{userId}", null, token);
	}


	public async Task<UnitResult<Error>> UnfreezeWalletAsync(Guid userId, CancellationToken token)
	{
		return await _httpService.PostAsync($"unfreezeWallet/{userId}", null, token);
	}


	public async Task<Result<BalanceResponse, Error>> GetBalanceAsync(Guid userId, CancellationToken token)
	{
		return await _httpService.GetAsync<BalanceResponse>($"getBalance/{userId}", token);
	}

	public async Task<Result<IEnumerable<TransactionResponse>, Error>> GetTransactionsAsync(Guid userId, int skip = 0, int take = 20, CancellationToken token = default)
	{
		return await _httpService.GetAsync<IEnumerable<TransactionResponse>>($"getTransactions/{userId}?skip={skip}&take={take}", token);
	}

	public async Task<Result<WalletResponse, Error>> GetWalletAsync(Guid userId, CancellationToken token)
	{
		return await _httpService.GetAsync<WalletResponse>($"getWallet/{userId}", token);
	}

	public async Task<Result<TransactionResponse, Error>> PurchaseAsync(Guid userId, PurchaseRequest request, CancellationToken token)
	{
		return await _httpService.PostAsync<TransactionResponse>($"purchase/{userId}", request, token);
	}

	public async Task<Result<TransactionResponse, Error>> RefundAsync(Guid userId, string orderId, decimal amount, CancellationToken token)
	{
		return await _httpService.PostAsync<TransactionResponse>($"refund/{userId}/{orderId}/{amount}", null, token);
	}


	public async Task<Result<TransactionResponse, Error>> WithdrawAsync(Guid userId, WithdrawRequest request, CancellationToken token)
	{
		return await _httpService.PostAsync<TransactionResponse>($"withdraw/{userId}", request, token);
	}



	public async Task<Result<CreatePaymentResponse, Error>> CreatePaymentAsync(Guid userId, CreatePaymentRequest request, CancellationToken token)
	{
		return await _httpService.PostAsync<CreatePaymentResponse>($"createPayment/{userId}", request, token);
	}
}