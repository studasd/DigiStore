using System;
using System.Collections.Generic;
using System.Text;
using DigiStore.UserService.Domain;
using DigiStore.UserService.Infrastructure.Postgres.Configurations;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DigiStore.UserService.Infrastructure.Postgres;


/// <summary>
/// DbContext for Accounts with Identity support
/// </summary>
public class UserDbContext : IdentityDbContext<UserDS, RoleDS, Guid>
{
	public UserDbContext(DbContextOptions<UserDbContext> options)
		: base(options)
	{
	}

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		base.OnModelCreating(modelBuilder);

		modelBuilder.ApplyConfigurationsFromAssembly(typeof(UserDbContext).Assembly);

		// Query filters (soft delete)
		modelBuilder.Entity<UserDS>()
			.HasQueryFilter(u => !u.IsDeleted);
	}
}
