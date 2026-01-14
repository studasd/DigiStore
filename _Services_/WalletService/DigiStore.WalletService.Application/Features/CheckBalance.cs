using CSharpFunctionalExtensions;
using DigiStore.Framework.Endpoints;
using DigiStore.SharedKernel;
using DigiStore.WalletService.Application.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace DigiStore.WalletService.Application.Features;

public sealed class CheckBalance : IEndpoint
{
	//[Authorize]
	public void MapEndpoint(IEndpointRouteBuilder app)
	{
		app.MapPost("checkBalance/{userId}/{amount}", async Task<EndpointResult<bool>> (
			[FromRoute] Guid userId,
			[FromRoute] decimal amount,
			[FromServices] CheckBalanceHandler handler,
			CancellationToken token) => await handler.Handle(userId, amount, token));
	}
}


public sealed class CheckBalanceHandler
{
	private readonly ILogger<CheckBalanceHandler> _logger;
	private readonly IWalletRepository _walletRepository;

	public CheckBalanceHandler(
		ILogger<CheckBalanceHandler> logger,
		IWalletRepository walletRepository)
	{
		_logger = logger;
		_walletRepository = walletRepository;
	}


	public async Task<Result<bool, Error>> Handle(Guid userId, decimal amount, CancellationToken ct)
	{
		if (amount <= 0)
		{
			return WalletErrors.InvalidAmount;
		}

		var wallet = await _walletRepository.GetByUserIdAsync(userId, ct);
		if (wallet == null)
		{
			return WalletErrors.WalletNotFound;
		}

		return wallet.Balance >= amount;
	}

}