using DigiStore.UserService.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace DigiStore.UserService.Domain;


/// <summary>
/// Custom user entity for Identity with support for multi-platform users
/// (Telegram, Web, other bots)
/// </summary>
public class UserDS : IdentityUser<Guid>
{
	/// <summary>
	/// Telegram unique user ID (can be null for web users)
	/// </summary>
	public long? TelegramId { get; set; }


	/// <summary>
	/// User's first name
	/// </summary>
	public string FirstName { get; set; } = string.Empty;

	/// <summary>
	/// User's last name
	/// </summary>
	public string LastName { get; set; } = string.Empty;

	/// <summary>
	/// Language preference (en, ru, etc.)
	/// </summary>
	public string LanguageCode { get; set; } = "en";

	/// <summary>
	/// Whether user is active across all platforms
	/// </summary>
	public bool IsActive { get; set; } = true;

	/// <summary>
	/// User registration source (Telegram, Web, etc.)
	/// </summary>
	public UserSource Source { get; set; } = UserSource.Telegram;

	/// <summary>
	/// Last activity timestamp
	/// </summary>
	public DateTime? LastActivityAt { get; set; }

	/// <summary>
	/// Creation timestamp (when user first registered)
	/// </summary>
	public DateTime CreatedAt { get; set; }

	/// <summary>
	/// Update timestamp
	/// </summary>
	public DateTime UpdatedAt { get; set; }

	/// <summary>
	/// Soft delete flag
	/// </summary>
	public bool IsDeleted { get; set; } = false;



	/// <summary>
	/// Navigation to roles (inherited from IdentityUser through IdentityUserRole)
	/// </summary>
	public ICollection<IdentityUserRole<Guid>> UserRoles { get; set; } = new List<IdentityUserRole<Guid>>();

	/// <summary>
	/// Get full name
	/// </summary>
	public string GetFullName() => $"{FirstName} {LastName}".Trim();

	/// <summary>
	/// Check if user is linked to Telegram
	/// </summary>
	public bool IsTelegramLinked() => TelegramId.HasValue;

}
