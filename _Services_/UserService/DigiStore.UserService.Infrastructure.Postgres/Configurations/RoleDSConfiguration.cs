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

		builder.HasIndex(r => r.Name)
			.IsUnique();

		// Seed system roles
		builder.HasData(
			new RoleDS
			{
				Id = Guid.Parse("00000000-0000-0000-0000-000000000001"),
				Name = "Admin",
				NormalizedName = "ADMIN",
				Description = "Administrator role with full access",
				ConcurrencyStamp = "17191573-3c54-4a6d-bf10-972650799fc3"
			},
			new RoleDS
			{
				Id = Guid.Parse("00000000-0000-0000-0000-000000000002"),
				Name = "Moderator",
				NormalizedName = "MODERATOR",
				Description = "Moderator role with limited access",
				ConcurrencyStamp = "3bd59661-a6c1-4763-9f93-05174f01de79"
			},
			new RoleDS
			{
				Id = Guid.Parse("00000000-0000-0000-0000-000000000003"),
				Name = "User",
				NormalizedName = "USER",
				Description = "Regular user role",
				ConcurrencyStamp = "df5d6b5b-23a8-4350-8542-f8af50611772"
			});
	}
}
