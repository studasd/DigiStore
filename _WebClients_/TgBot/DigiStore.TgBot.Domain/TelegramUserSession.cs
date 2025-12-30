using System;
using System.Collections.Generic;
using System.Text;

namespace DigiStore.TgBot.Domain;


/// <summary>
/// Telegram user session state
/// </summary>
public class TelegramUserSession
{
	public long TelegramId { get; set; }
	public Guid? UserId { get; set; }
	public string CurrentState { get; set; } = BotState.Start;
	public string? LanguageCode { get; set; } = "en";
	public Dictionary<string, object> Data { get; set; } = new();
	public DateTime LastActivity { get; set; } = DateTime.UtcNow;
	public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

	// Кэш профиля пользователя (для быстрого доступа)
	public CachedUserProfile? CachedProfile { get; set; }

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
