using System;
using System.Collections.Generic;
using System.Text;

namespace DigiStore.UserService.Contracts.Requests;


/// <summary>
/// Request to link Telegram account to existing user
/// </summary>
public record LinkTelegramRequest
{
	/// <summary>
	/// User ID to link Telegram to
	/// </summary>
	public Guid UserId { get; set; }

	/// <summary>
	/// Telegram user ID
	/// </summary>
	public long TelegramId { get; set; }

	/// <summary>
	/// Telegram username (optional)
	/// </summary>
	public string? TelegramUsername { get; set; }

	/// <summary>
	/// First name from Telegram
	/// </summary>
	public string? FirstName { get; set; }

	/// <summary>
	/// Last name from Telegram
	/// </summary>
	public string? LastName { get; set; }
}
