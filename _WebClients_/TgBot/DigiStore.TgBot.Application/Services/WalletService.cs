using CSharpFunctionalExtensions;
using DigiStore.Enums;
using DigiStore.SharedKernel;
using DigiStore.TgBot.Application.Constants;
using DigiStore.TgBot.Application.DTOs;
using DigiStore.TgBot.Application.Interfaces.Services;
using DigiStore.WalletService.Contracts.HttpClients;
using DigiStore.WalletService.Contracts.Requests.Payments;
using DigiStore.WalletService.Contracts.Responses;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace DigiStore.TgBot.Application.Services;


public class WalletService : IWalletService
{
    private readonly IWalletHttpClient _walletHttpClient;
    private readonly ILogger<WalletService> _logger;

	public WalletService(
		IWalletHttpClient walletHttpClient,
		IConfiguration configuration,
		ILogger<WalletService> logger)
	{
        _walletHttpClient = walletHttpClient;
        _logger = logger;
	}


	public async Task<Result<string, Error>> CreatePaymentAsync(Guid userId, PaymentAggregators paymentAggregator, decimal amount, CancellationToken token)
	{
		var req = new CreatePaymentRequest(paymentAggregator, amount, "Пополнение баланса");

		var result = await _walletHttpClient.CreatePaymentAsync(userId, req, token);
		if (result.IsFailure)
			return result.Error;

		return result.Value.RredirectUrl;
	}


	public async Task<Result<BalanceDto, Error>> GetBalanceAsync(Guid userId, CancellationToken token)
	{
		try
		{
			// Заглушка

			var result = await _walletHttpClient.GetBalanceAsync(userId, token);
			if(result.IsFailure)
				return result.Error;

			return new BalanceDto(result.Value.Value);

			//return new BalanceDto
			//{
			//	Balance = -1.11m,
			//	Currency = "RUB"
			//};

			//var url = $"{_walletServiceUrl}/api/wallet/{userId}";
			//var response = await _httpClient.GetAsync(url, ct);

			//if (!response.IsSuccessStatusCode)
			//{
			//	_logger.LogWarning("Failed to get balance for user ID: {UserId}", userId);
			//	return TgBotErrors.OperationFailed;
			//}

			//var content = await response.Content.ReadAsStringAsync(ct);
			//var wallet = JsonSerializer.Deserialize<BalanceDto>(content);

			//return wallet;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error getting balance for user ID: {UserId}", userId);
			return Error.Failure("bot.wallet_service_error", ex.Message);
		}
	}


	public async Task<Result<IEnumerable<TransactionResponse>, Error>> GetTransactionsAsync(
		Guid userId,
		int take = 10,
		CancellationToken token = default)
	{
		//try
		//{
			var result = await _walletHttpClient.GetTransactionsAsync(userId, 0, take, token);

			return result;

			//var url = $"{_walletServiceUrl}/api/wallet/{userId}/transactions?skip=0&take={take}";
			//var response = await _httpClient.GetAsync(url, ct);

			//if (!response.IsSuccessStatusCode)
			//{
			//	_logger.LogWarning("Failed to get transactions for user ID: {UserId}", userId);
			//	return TgBotErrors.OperationFailed;
			//}

			//var content = await response.Content.ReadAsStringAsync(ct);
			//var transactions = JsonSerializer.Deserialize<List<TransactionDto>>(content);

			//return transactions ?? new();
		//}
		//catch (Exception ex)
		//{
		//	_logger.LogError(ex, "Error getting transactions for user ID: {UserId}", userId);
		//	return Error.Failure("bot.wallet_service_error", ex.Message);
		//}
	}


	public async Task<Result<bool, Error>> InitiateWithdrawalAsync(
		Guid userId,
		decimal amount,
		CancellationToken token)
	{
		//try
		//{
		//	var url = $"{_walletServiceUrl}/api/wallet/{userId}/withdraw";
		//	var request = new { amount = amount, description = "Withdrawal via Telegram bot" };

		//	var json = JsonSerializer.Serialize(request);
		//	var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

		//	var response = await _httpClient.PostAsync(url, content, ct);

		//	if (!response.IsSuccessStatusCode)
		//	{
		//		_logger.LogWarning("Failed to initiate withdrawal for user ID: {UserId}", userId);
		//		return TgBotErrors.OperationFailed;
		//	}

			return true;
		//}
		//catch (Exception ex)
		//{
		//	_logger.LogError(ex, "Error initiating withdrawal for user ID: {UserId}", userId);
		//	return Error.Failure("bot.wallet_service_error", ex.Message);
		//}
	}
}
