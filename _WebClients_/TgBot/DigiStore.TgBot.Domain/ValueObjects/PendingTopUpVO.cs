namespace DigiStore.TgBot.Domain.ValueObjects;

public record PendingTopUpVO(string? Aggregator, decimal? Amount, long? ChatId, int? MessageId);
