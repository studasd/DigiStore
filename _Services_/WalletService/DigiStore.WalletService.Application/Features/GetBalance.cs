using CSharpFunctionalExtensions;
using DigiStore.Framework.Endpoints;
using DigiStore.SharedKernel;
using DigiStore.WalletService.Application.Interfaces;
using DigiStore.WalletService.Contracts.Responses;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;

namespace DigiStore.WalletService.Application.Features;

public sealed class GetBalance : IEndpoint
{
	//[Authorize]
	public void MapEndpoint(IEndpointRouteBuilder app)
	{
		app.MapGet("getBalance/{userId}", async Task<EndpointResult<BalanceResponse>> (
			[FromRoute] Guid userId,
			[FromServices] GetBalanceHandler handler,
			CancellationToken token) => await handler.Handle(userId, token));
	}
}


public sealed class GetBalanceHandler : IWalletServiceHandler
{
	private readonly ILogger<GetBalanceHandler> _logger;
	private readonly IWalletRepository _walletRepository;

	public GetBalanceHandler(
		ILogger<GetBalanceHandler> logger,
		IWalletRepository walletRepository)
	{
		_logger = logger;
		_walletRepository = walletRepository;
	}


	public async Task<Result<BalanceResponse, Error>> Handle(Guid userId, CancellationToken ct)
	{
		try
		{
			//var cacheKey = string.Format(BalanceCacheKeyFormat, userId);
			//var cached = await _cache.GetAsync<decimal?>(cacheKey, ct);
			//if (cached.HasValue)
			//{
			//	return Result<decimal>.Success(cached.Value);
			//}
			var wallet = await _walletRepository.GetOrCreateByUserIdAsync(userId, ct);
			if (wallet == null)
			{
				return WalletErrors.WalletNotFound;
			}
			//await _cache.SetAsync(cacheKey, wallet.Balance, TimeSpan.FromMinutes(1), ct);
			return new BalanceResponse(wallet.Balance);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error getting balance for user: {UserId}", userId);
			return Error.Internal("wallet.balance_error", "Error getting balance");
		}
	}

}