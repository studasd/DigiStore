using DigiStore.WalletService.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace DigiStore.WalletService.Application.Services;

public class WalletService : IWalletService
{
	private readonly IWalletRepository _repository;
	//private readonly ICacheService _cache;
	private readonly ILogger<WalletService> _logger;
	private const string WalletCacheKeyFormat = "wallet:{0}";
	private const string BalanceCacheKeyFormat = "wallet:balance:{0}";
	private readonly TimeSpan _walletCacheExpiration = TimeSpan.FromMinutes(5);

	public WalletService(
		IWalletRepository repository,
		//ICacheService cache,
		ILogger<WalletService> logger)
	{
		_repository = repository;
		//_cache = cache;
		_logger = logger;
	}



	private async Task InvalidateWalletCacheAsync(Guid userId, CancellationToken ct)
	{
		//await _cache.RemoveAsync(string.Format(WalletCacheKeyFormat, userId), ct);
		//await _cache.RemoveAsync(string.Format(BalanceCacheKeyFormat, userId), ct);
	}
}