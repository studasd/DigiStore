using CSharpFunctionalExtensions;
using DigiStore.SharedKernel;
using DigiStore.SharedKernel.HttpServices;
using DigiStore.WalletService.Contracts.Requests;
using DigiStore.WalletService.Contracts.Responses;
using Microsoft.Extensions.Logging;

namespace DigiStore.UserService.Contracts.HttpClients;

internal sealed class WalletHttpClient : IWalletHttpClient
{
	private readonly ILogger<WalletHttpClient> _logger;
    private readonly HttpService _httpService;

    public WalletHttpClient(ILogger<WalletHttpClient> logger, IHttpServiceFactory httpServiceFactory)
	{
		_logger = logger;
        _httpService = httpServiceFactory.CreateHttpService<WalletHttpClient>();
    }


	public async Task<Result<CheckBalanceResponse, Error>> CheckBalanceAsync(Guid userId, decimal amount, CancellationToken cancellationToken)
	{
		return await _httpService.PostAsync<CheckBalanceResponse>($"checkBalance/{userId}/{amount}", null, cancellationToken);
	}
	
	public async Task<Result<TransactionResponse, Error>> DepositAsync(Guid userId, DepositRequest request, CancellationToken cancellationToken)
	{
		return await _httpService.PostAsync<TransactionResponse>($"deposit/{userId}", request,  cancellationToken);
	}
	
	
	public async Task<UnitResult<Error>> FreezeWalletAsync(Guid userId, CancellationToken cancellationToken)
	{
		return await _httpService.PostAsync($"freezeWallet/{userId}", null, cancellationToken);
	}


	public async Task<UnitResult<Error>> UnfreezeWalletAsync(Guid userId, CancellationToken cancellationToken)
	{
		return await _httpService.PostAsync($"unfreezeWallet/{userId}", null, cancellationToken);
	}


	public async Task<Result<BalanceResponse, Error>> GetBalanceAsync(Guid userId, CancellationToken cancellationToken)
	{
		return await _httpService.GetAsync<BalanceResponse>($"getBalance/{userId}", cancellationToken);
	}

	public async Task<Result<IEnumerable<TransactionResponse>, Error>> GetTransactionsAsync(Guid userId, int skip = 0, int take = 20, CancellationToken cancellationToken = default)
	{
		return await _httpService.GetAsync<IEnumerable<TransactionResponse>>($"getTransactions/{userId}?skip={skip}&take={take}", cancellationToken);
	}

	public async Task<Result<WalletResponse, Error>> GetWalletAsync(Guid userId, CancellationToken cancellationToken)
	{
		return await _httpService.GetAsync<WalletResponse>($"getWallet/{userId}", cancellationToken);
	}

	public async Task<Result<TransactionResponse, Error>> PurchaseAsync(Guid userId, PurchaseRequest request, CancellationToken cancellationToken)
	{
		return await _httpService.PostAsync<TransactionResponse>($"purchase/{userId}", request, cancellationToken);
	}

	public async Task<Result<TransactionResponse, Error>> RefundAsync(Guid userId, string orderId, decimal amount, CancellationToken cancellationToken)
	{
		return await _httpService.PostAsync<TransactionResponse>($"refund/{userId}/{orderId}/{amount}", null, cancellationToken);
	}


	public async Task<Result<TransactionResponse, Error>> WithdrawAsync(Guid userId, WithdrawRequest request, CancellationToken cancellationToken)
	{
		return await _httpService.PostAsync<TransactionResponse>($"withdraw/{userId}", request, cancellationToken);
	}

}