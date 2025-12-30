using DigiStore.UserService.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace DigiStore.UserService.Infrastructure.Postgres.Configurations;


/// <summary>
/// EF Core configuration for RoleDS entity
/// </summary>
public class RoleDSConfiguration : IEntityTypeConfiguration<RoleDS>
{
	public void Configure(EntityTypeBuilder<RoleDS> builder)
	{
		builder.ToTable("Roles");

		builder.Property(r => r.Description)
			.HasMaxLength(500);

		builder.Property(r => r.IsSystem)
			.IsRequired()
			.HasDefaultValue(false);

		builder.Property(r => r.DisplayOrder)
			.IsRequired()
			.HasDefaultValue(0);

		builder.HasIndex(r => r.Name)
			.IsUnique();

		builder.HasIndex(r => r.IsSystem);

		// Seed system roles
		builder.HasData(
			new RoleDS
			{
				Id = Guid.Parse("00000000-0000-0000-0000-000000000001"),
				Name = "Admin",
				NormalizedName = "ADMIN",
				Description = "Administrator role with full access",
				IsSystem = true,
				DisplayOrder = 1
			},
			new RoleDS
			{
				Id = Guid.Parse("00000000-0000-0000-0000-000000000002"),
				Name = "Moderator",
				NormalizedName = "MODERATOR",
				Description = "Moderator role with limited access",
				IsSystem = true,
				DisplayOrder = 2
			},
			new RoleDS
			{
				Id = Guid.Parse("00000000-0000-0000-0000-000000000003"),
				Name = "User",
				NormalizedName = "USER",
				Description = "Regular user role",
				IsSystem = true,
				DisplayOrder = 3
			});
	}
}
