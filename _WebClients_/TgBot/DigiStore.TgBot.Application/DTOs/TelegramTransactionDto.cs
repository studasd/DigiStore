namespace DigiStore.TgBot.Application.DTOs;

public class TelegramTransactionDto
{
	public decimal Amount { get; set; }
	public string Type { get; set; } = string.Empty;
	public string Description { get; set; } = string.Empty;
	public DateTime CreatedAt { get; set; }
}
