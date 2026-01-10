namespace DigiStore.TgBot.Domain;

public class TgUserSession
{
    public Guid Id { get; set; } = Guid.NewGuid();

	public Guid UserId { get; set; }

	public long TelegramId { get; set; }

    public string CurrentState { get; set; } = string.Empty;

    public string LanguageCode { get; set; } = "en";

    public Dictionary<string, object> Data { get; set; } = new Dictionary<string, object>();

	public CachedUserProfile? CachedProfile { get; set; }

    public DateTime LastActivity { get; set; }

    public DateTime CreatedAt { get; set; }


	public void UpdateActivity()
	{
		LastActivity = DateTime.UtcNow;
	}

	public void SetState(string state)
	{
		CurrentState = state;
		UpdateActivity();
	}

	public void SetData(string key, object value)
	{
		Data[key] = value;
		UpdateActivity();
	}

	public object? GetData(string key)
	{
		return Data.TryGetValue(key, out var value) ? value : null;
	}

	public void ClearData(string key)
	{
		Data.Remove(key);
	}

	public void ClearAllData()
	{
		Data.Clear();
	}
}
