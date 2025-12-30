using Microsoft.AspNetCore.Identity;

namespace DigiStore.UserService.Domain;


/// <summary>
/// Custom role entity for Identity
/// </summary>
public class RoleDS : IdentityRole<Guid>
{
	/// <summary>
	/// Role description
	/// </summary>
	public string Description { get; set; } = string.Empty;

	/// <summary>
	/// Whether role is system role (cannot be deleted)
	/// </summary>
	public bool IsSystem { get; set; } = false;

	/// <summary>
	/// Order for display purposes
	/// </summary>
	public int DisplayOrder { get; set; }


	/// <summary>
	/// Navigation to users
	/// </summary>
	public ICollection<IdentityUserRole<Guid>> UserRoles { get; set; } = new List<IdentityUserRole<Guid>>();

	/// <summary>
	/// Navigation to permissions
	/// </summary>
	public ICollection<IdentityRoleClaim<Guid>> RoleClaims { get; set; } = new List<IdentityRoleClaim<Guid>>();
}
