namespace DigiStore.TgBot.Domain.ValueObjects;

public record PendingPaymentMessageVO(long ChatId, int MessageId, decimal Amount, string? Aggregator);
