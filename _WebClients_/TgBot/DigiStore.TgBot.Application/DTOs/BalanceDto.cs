namespace DigiStore.TgBot.Application.DTOs;

public record BalanceDto
(
	decimal Balance,
	string Currency = "RUB",
	decimal TotalDeposited = 0,
	decimal TotalWithdrawn = 0
);