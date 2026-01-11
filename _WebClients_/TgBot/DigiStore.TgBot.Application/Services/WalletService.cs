using CSharpFunctionalExtensions;
using DigiStore.SharedKernel;
using DigiStore.TgBot.Application.Constants;
using DigiStore.TgBot.Application.DTOs;
using DigiStore.TgBot.Application.Interfaces.Services;
using DigiStore.TgBot.Application.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace DigiStore.TgBot.Application.Services;


public class WalletService : IWalletService
{
	private readonly HttpClient _httpClient;
	private readonly ILogger<WalletService> _logger;
	private readonly string _walletServiceUrl;

	public WalletService(
		HttpClient httpClient,
		IConfiguration configuration,
		IOptions<ServiceOptions> options,
		ILogger<WalletService> logger)
	{
		_httpClient = httpClient;
		_logger = logger;
		_walletServiceUrl = options.Value.WalletServiceUrl // configuration["Services:WalletService:Url"]
			?? throw new InvalidOperationException("WalletService URL not configured");
	}


	public async Task<Result<BalanceDto, Error>> GetBalanceAsync(Guid userId, CancellationToken ct = default)
	{
		try
		{
			var url = $"{_walletServiceUrl}/api/wallet/{userId}";
			var response = await _httpClient.GetAsync(url, ct);

			if (!response.IsSuccessStatusCode)
			{
				_logger.LogWarning("Failed to get balance for user ID: {UserId}", userId);
				return TgBotErrors.OperationFailed;
			}

			var content = await response.Content.ReadAsStringAsync(ct);
			var wallet = JsonSerializer.Deserialize<BalanceDto>(content);

			return wallet;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error getting balance for user ID: {UserId}", userId);
			return Error.Failure("bot.wallet_service_error", ex.Message);
		}
	}


	public async Task<Result<IEnumerable<TransactionDto>, Error>> GetTransactionsAsync(
		Guid userId,
		int take = 10,
		CancellationToken ct = default)
	{
		try
		{
			var url = $"{_walletServiceUrl}/api/wallet/{userId}/transactions?skip=0&take={take}";
			var response = await _httpClient.GetAsync(url, ct);

			if (!response.IsSuccessStatusCode)
			{
				_logger.LogWarning("Failed to get transactions for user ID: {UserId}", userId);
				return TgBotErrors.OperationFailed;
			}

			var content = await response.Content.ReadAsStringAsync(ct);
			var transactions = JsonSerializer.Deserialize<List<TransactionDto>>(content);

			return transactions ?? new();
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error getting transactions for user ID: {UserId}", userId);
			return Error.Failure("bot.wallet_service_error", ex.Message);
		}
	}


	public async Task<Result<bool, Error>> InitiateWithdrawalAsync(
		Guid userId,
		decimal amount,
		CancellationToken ct = default)
	{
		try
		{
			var url = $"{_walletServiceUrl}/api/wallet/{userId}/withdraw";
			var request = new { amount = amount, description = "Withdrawal via Telegram bot" };

			var json = JsonSerializer.Serialize(request);
			var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

			var response = await _httpClient.PostAsync(url, content, ct);

			if (!response.IsSuccessStatusCode)
			{
				_logger.LogWarning("Failed to initiate withdrawal for user ID: {UserId}", userId);
				return TgBotErrors.OperationFailed;
			}

			return true;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error initiating withdrawal for user ID: {UserId}", userId);
			return Error.Failure("bot.wallet_service_error", ex.Message);
		}
	}
}
