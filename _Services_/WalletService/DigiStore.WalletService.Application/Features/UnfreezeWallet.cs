using CSharpFunctionalExtensions;
using DigiStore.Framework.Endpoints;
using DigiStore.SharedKernel;
using DigiStore.WalletService.Application.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;

namespace DigiStore.WalletService.Application.Features;

public sealed class UnfreezeWallet : IEndpoint
{
	//[Authorize]
	public void MapEndpoint(IEndpointRouteBuilder app)
	{
		app.MapPost("unfreezeWallet/{userId}", async Task<EndpointResult> (
			[FromRoute] Guid userId,
			[FromServices] UnfreezeWalletHandler handler,
			CancellationToken token) => await handler.Handle(userId, token));
	}
}


public sealed class UnfreezeWalletHandler : IWalletServiceHandler
{
	private readonly ILogger<UnfreezeWalletHandler> _logger;
	private readonly IWalletRepository _walletRepository;

	public UnfreezeWalletHandler(
		ILogger<UnfreezeWalletHandler> logger,
		IWalletRepository walletRepository)
	{
		_logger = logger;
		_walletRepository = walletRepository;
	}


	public async Task<UnitResult<Error>> Handle(Guid userId, CancellationToken ct = default)
	{
		try
		{
			var walletResult = await _walletRepository.GetOrCreateByUserIdAsync(userId, ct);
			if (walletResult.IsFailure)
				return walletResult.Error;

			var wallet = walletResult.Value;

			wallet.Unfreeze();

			await _walletRepository.UpdateAsync(wallet, ct);

			//await InvalidateWalletCacheAsync(userId, ct);

			_logger.LogInformation("Wallet unfrozen for user: {UserId}", userId);
			return Result.Success<Error>();
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error unfreezing wallet for user: {UserId}", userId);
			return Error.Internal("wallet.unfreeze_error", "Error unfreezing wallet for user");
		}
	}
}