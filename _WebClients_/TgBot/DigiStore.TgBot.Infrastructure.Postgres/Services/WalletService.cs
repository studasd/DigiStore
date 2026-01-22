using CSharpFunctionalExtensions;
using DigiStore.Enums;
using DigiStore.SharedKernel;
using DigiStore.TgBot.Application.DTOs;
using DigiStore.TgBot.Application.Interfaces.Repositories;
using DigiStore.TgBot.Application.Interfaces.Services;
using DigiStore.WalletService.Contracts.HttpClients;
using DigiStore.WalletService.Contracts.Requests.Payments;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace DigiStore.TgBot.Infrastructure.Postgres.Services;


public class WalletService : IWalletService
{
    private readonly IWalletHttpClient _walletHttpClient;
    private readonly ITgUserRepository _tgUserRepository;
    private readonly ILogger<WalletService> _logger;

	public WalletService(
		IWalletHttpClient walletHttpClient,
		ITgUserRepository tgUserRepository,
		IConfiguration configuration,
		ILogger<WalletService> logger)
	{
        _walletHttpClient = walletHttpClient;
        _tgUserRepository = tgUserRepository;
        _logger = logger;
	}


	public async Task<Result<(Guid paymentId, string redirectUrl), Error>> CreatePaymentAsync(Guid userId, PaymentAggregators paymentAggregator, decimal amount, CancellationToken token)
	{
		var userTgResult = await _tgUserRepository.GetByUserIdAsync(userId, token);
		if (userTgResult.IsFailure)
			return userTgResult.Error;	

		var userTg = userTgResult.Value;
		var returnUrl = $"https://t.me/{userTg.Username}"; // возврат после оплаты для юзера
		
		var req = new CreatePaymentRequest(paymentAggregator, amount, $"Пополнение баланса #{userTg.TelegramId}", returnUrl);

		var result = await _walletHttpClient.CreatePaymentAsync(userId, req, token);
		if (result.IsFailure)
			return result.Error;

		return (result.Value.PaymentId, result.Value.RredirectUrl);
	}


	public async Task<Result<BalanceDto, Error>> GetBalanceAsync(Guid userId, CancellationToken token)
	{
		var result = await _walletHttpClient.GetBalanceAsync(userId, token);
		if(result.IsFailure)
			return result.Error;

		return new BalanceDto(result.Value.Value);
	}


	public async Task<Result<IEnumerable<TransactionDto>, Error>> GetTransactionsAsync(
		Guid userId,
		int take = 10,
		CancellationToken token = default)
	{
		var result = await _walletHttpClient.GetTransactionsAsync(userId, 0, take, token);
		if (result.IsFailure)
			return result.Error;

		var trans = result.Value;

		return trans.Select(t => new TransactionDto
			(t.Id, 
			t.WalletId, 
			t.Amount, 
			t.Type, 
			t.Status, 
			t.Description, 
			t.BalanceAfter, 
			t.CreatedAt
			)).ToList();
	}


	public async Task<Result<bool, Error>> InitiateWithdrawalAsync(
		Guid userId,
		decimal amount,
		CancellationToken token)
	{
		return true;
	}
}
