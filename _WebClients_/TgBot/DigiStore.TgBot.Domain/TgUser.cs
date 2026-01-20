namespace DigiStore.TgBot.Domain;

public class TgUser
{
	public Guid UserId { get; set; }

	public long TelegramId { get; set; }

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string? Username { get; set; }

    public bool IsActive { get; set; } = true;

    
    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

	public string GetFullName() => $"{FirstName} {LastName}".Trim();
}
