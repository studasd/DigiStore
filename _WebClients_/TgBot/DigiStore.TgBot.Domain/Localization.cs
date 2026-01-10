using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DigiStore.TgBot.Domain;

public class Localization
{
    public string Key { get; set; } = string.Empty; // localization key, primary key

    // Language columns
    public string? En { get; set; }
    public string? Ru { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
