using System;
using System.Collections.Generic;
using System.Text;

namespace DigiStore.TgBot.Application.DTOs;


public class TelegramUserDto
{
	public Guid Id { get; set; }
	public long TelegramId { get; set; }
	public string Email { get; set; } = string.Empty;
	public string FullName { get; set; } = string.Empty;
	public string? TelegramUsername { get; set; }
	public string LanguageCode { get; set; } = "en";
	public bool IsActive { get; set; }
	public List<string> Roles { get; set; } = new();
}

public class TelegramBalanceDto
{
	public decimal Balance { get; set; }
	public string Currency { get; set; } = "RUB";
	public decimal TotalDeposited { get; set; }
	public decimal TotalWithdrawn { get; set; }
}

public class TelegramTransactionDto
{
	public decimal Amount { get; set; }
	public string Type { get; set; } = string.Empty;
	public string Description { get; set; } = string.Empty;
	public DateTime CreatedAt { get; set; }
}
