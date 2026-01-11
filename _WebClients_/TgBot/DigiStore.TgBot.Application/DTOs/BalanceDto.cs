namespace DigiStore.TgBot.Application.DTOs;

public class BalanceDto
{
	public decimal Balance { get; set; }
	public string Currency { get; set; } = "RUB";
	public decimal TotalDeposited { get; set; }
	public decimal TotalWithdrawn { get; set; }
}
