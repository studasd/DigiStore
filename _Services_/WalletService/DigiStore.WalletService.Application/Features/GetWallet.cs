using CSharpFunctionalExtensions;
using DigiStore.Framework.Endpoints;
using DigiStore.SharedKernel;
using DigiStore.WalletService.Application.Extensions;
using DigiStore.WalletService.Application.Interfaces;
using DigiStore.WalletService.Contracts.Responses;
using DigiStore.WalletService.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace DigiStore.WalletService.Application.Features;

public sealed class GetWallet : IEndpoint
{
	//[Authorize]
	public void MapEndpoint(IEndpointRouteBuilder app)
	{
		app.MapGet("getWallet/{userId}", async Task<EndpointResult<WalletResponse>> (
			[FromRoute] Guid userId,
			[FromServices] GetWalletHandler handler,
			CancellationToken token) => await handler.Handle(userId, token));
	}
}


public sealed class GetWalletHandler : IWalletServiceHandler
{
	private readonly ILogger<GetWalletHandler> _logger;
    private readonly IWalletRepository _walletRepository;

	public GetWalletHandler(
		ILogger<GetWalletHandler> logger,
		IWalletRepository walletRepository)
	{
		_logger = logger;
        _walletRepository = walletRepository;
	}


	public async Task<Result<WalletResponse, Error>> Handle(Guid userId, CancellationToken ct)
	{
		try
		{
			//var cacheKey = string.Format(WalletCacheKeyFormat, userId);
			//var cached = await _cache.GetAsync<WalletResponse>(cacheKey, ct);
			//if (cached != null)
			//{
			//	return Result<WalletResponse>.Success(cached);
			//}
			var wallet = await _walletRepository.GetOrCreateByUserIdAsync(userId, ct);
			if (wallet == null)
			{
				_logger.LogWarning("Wallet not found for user: {UserId}", userId);
				return WalletErrors.WalletNotFound;
			}
			var response = wallet.MapToResponse();
			//await _cache.SetAsync(cacheKey, response, _walletCacheExpiration, ct);
			return response;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error getting wallet for user: {UserId}", userId);
			return Error.Internal("wallet.retrieval_error", "Error getting wallet");
		}
	}

}