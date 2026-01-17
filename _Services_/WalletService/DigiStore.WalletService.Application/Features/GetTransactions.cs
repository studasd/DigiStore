using CSharpFunctionalExtensions;
using DigiStore.Framework.Endpoints;
using DigiStore.SharedKernel;
using DigiStore.WalletService.Application.Extensions;
using DigiStore.WalletService.Application.Interfaces;
using DigiStore.WalletService.Contracts.Responses;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace DigiStore.WalletService.Application.Features;

public sealed class GetTransactions : IEndpoint
{
	//[Authorize]
	public void MapEndpoint(IEndpointRouteBuilder app)
	{
		app.MapGet("getTransactions/{userId}", async Task<EndpointResult<IEnumerable<TransactionResponse>>> (
			[FromRoute] Guid userId,
			[FromServices] GetTransactionsHandler handler,
			[FromQuery] int skip = 0,
			[FromQuery] int take = 20,
			CancellationToken token = default) => await handler.Handle(userId, skip, take, token));
	}
}


public sealed class GetTransactionsHandler : IWalletServiceHandler
{
	private readonly ILogger<GetTransactionsHandler> _logger;
	private readonly IWalletRepository _walletRepository;

	public GetTransactionsHandler(
		ILogger<GetTransactionsHandler> logger,
		IWalletRepository walletRepository)
	{
		_logger = logger;
		_walletRepository = walletRepository;
	}


	public async Task<Result<IEnumerable<TransactionResponse>, Error>> Handle(Guid userId, int skip = 0, int take = 20, CancellationToken ct = default)
	{
		try
		{
			var wallet = await _walletRepository.GetOrCreateByUserIdAsync(userId, ct);
			if (wallet == null)
			{
				return WalletErrors.WalletNotFound;
			}
			var transactions = await _walletRepository.GetTransactionsByWalletIdAsync(
			wallet.Id, skip, take, ct);

			var response = transactions.Select(x => x.MapToResponse()).ToList();
			return response;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error getting transactions for user: {UserId}", userId);
			return Error.Internal("wallet.transactions_error", "Error getting transactions for user");
		}
	}

}