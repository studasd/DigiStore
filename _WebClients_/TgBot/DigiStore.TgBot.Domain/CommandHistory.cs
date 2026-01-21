using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DigiStore.TgBot.Domain;

public class CommandHistory
{
    public Guid Id { get; init; } = Guid.NewGuid();

    public long TelegramId { get; init; }

    public string? Command { get; init; } = string.Empty;

    public string? Message { get; init; } = string.Empty;

	public DateTime Timestamp { get; init; }
}
