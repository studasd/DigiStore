using CSharpFunctionalExtensions;
using StudCoreKit.Framework.Endpoints;
using StudCoreKit.SharedKernel;
using DigiStore.WalletService.Application.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;

namespace DigiStore.WalletService.Application.Features;

public sealed class FreezeWallet : IEndpoint
{
	//[Authorize]
	public void MapEndpoint(IEndpointRouteBuilder app)
	{
		app.MapPost("freezeWallet/{userId}", async Task<EndpointResult> (
			[FromRoute] Guid userId,
			[FromServices] FreezeWalletHandler handler,
			CancellationToken token) => await handler.Handle(userId, token));
	}
}


public sealed class FreezeWalletHandler : IWalletServiceHandler
{
	private readonly ILogger<FreezeWalletHandler> _logger;
	private readonly IWalletRepository _walletRepository;

	public FreezeWalletHandler(
		ILogger<FreezeWalletHandler> logger,
		IWalletRepository walletRepository)
	{
		_logger = logger;
		_walletRepository = walletRepository;
	}


	public async Task<UnitResult<Error>> Handle(Guid userId, CancellationToken token)
	{
		var walletResult = await _walletRepository.GetOrCreateByUserIdAsync(userId, token);
		if (walletResult.IsFailure)
			return walletResult.Error;

		var wallet = walletResult.Value;
		wallet.Freeze();

		var updateResult = await _walletRepository.UpdateAsync(wallet, token);
		if(updateResult.IsFailure)
			return updateResult.Error;

		//await InvalidateWalletCacheAsync(userId, ct);

		_logger.LogInformation("Wallet frozen for user: {UserId}", userId);
		return Result.Success<Error>();
	}
}