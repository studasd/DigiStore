using DigiStore.UserService.Contracts.Enums;

namespace DigiStore.TgBot.Application.DTOs;

/// <summary>
/// DTO для отображения полного профиля пользователя в телеграм
/// </summary>
public class ProfileDisplayDto
{
	/// <summary>
	/// Telegram ID пользователя
	/// </summary>
	public long TelegramId { get; set; }
	/// <summary>
	/// UUID пользователя в системе
	/// </summary>
	public Guid UserId { get; set; }
	/// <summary>
	/// Полное имя пользователя
	/// </summary>
	public string FullName { get; set; } = string.Empty;
	/// <summary>
	/// Email
	/// </summary>
	public string Email { get; set; } = string.Empty;
	/// <summary>
	/// Telegram username (@username)
	/// </summary>
	public string? Username { get; set; }
	/// <summary>
	/// Текущий баланс
	/// </summary>
	public decimal Balance { get; set; }
	/// <summary>
	/// Валюта
	/// </summary>
	public string Currency { get; set; } = "RUB";
	/// <summary>
	/// Язык пользователя
	/// </summary>
	public LanguageCodes LangCode { get; set; } = LanguageCodes.en;
	/// <summary>
	/// Активен ли аккаунт
	/// </summary>
	public bool IsActive { get; set; }
	/// <summary>
	/// Роли пользователя
	/// </summary>
	public List<string> Roles { get; set; } = new();
	/// <summary>
	/// Дата регистрации
	/// </summary>
	public DateTime CreatedAt { get; set; }
	/// <summary>
	/// Последнее обновление
	/// </summary>
	public DateTime UpdatedAt { get; set; }
	/// <summary>
	/// Когда пользователь был активен последний раз
	/// </summary>
	public DateTime? LastActivityAt { get; set; }
}
