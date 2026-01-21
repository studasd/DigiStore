using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DigiStore.TgBot.Domain;

public class Localization
{
    public string Key { get; init; } = string.Empty; // localization key, primary key

    // Language columns
    public string? Ru { get; set; }
    public string? En { get; set; }

	public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
