using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DigiStore.TgBot.Domain;

public class CommandHistory
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public long TelegramId { get; set; }

    public string? Command { get; set; } = string.Empty;

    public string? Message { get; set; } = string.Empty;

	public DateTime Timestamp { get; set; }
}
