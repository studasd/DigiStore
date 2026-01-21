using Microsoft.AspNetCore.Identity;

namespace DigiStore.UserService.Domain;


/// <summary>
/// Custom role entity for Identity
/// </summary>
public class RoleDS : IdentityRole<Guid>
{
	public const string Admin =		"Admin";
	public const string Moderator = "Moderator";
	public const string User =		"User";


	/// <summary>
	/// Role description
	/// </summary>
	public string Description { get; init; } = string.Empty;

	

	/// <summary>
	/// Navigation to users
	/// </summary>
	public ICollection<IdentityUserRole<Guid>> UserRoles { get; init; } = new List<IdentityUserRole<Guid>>();

	/// <summary>
	/// Navigation to permissions
	/// </summary>
	public ICollection<IdentityRoleClaim<Guid>> RoleClaims { get; init; } = new List<IdentityRoleClaim<Guid>>();
}
