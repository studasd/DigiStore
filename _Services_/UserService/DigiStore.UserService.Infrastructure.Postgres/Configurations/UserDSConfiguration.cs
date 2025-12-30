using DigiStore.UserService.Domain;
using DigiStore.UserService.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DigiStore.UserService.Infrastructure.Postgres.Configurations;


/// <summary>
/// EF Core configuration for UserDS entity
/// </summary>
public class UserDSConfiguration : IEntityTypeConfiguration<UserDS>
{
	public void Configure(EntityTypeBuilder<UserDS> builder)
	{
		// Table
		builder.ToTable("Users");

		// Primary Key
		builder.HasKey(u => u.Id);

		// Properties
		builder.Property(u => u.Id)
			.ValueGeneratedNever();

		builder.Property(u => u.FirstName)
			.HasMaxLength(100)
			.IsRequired();

		builder.Property(u => u.LastName)
			.HasMaxLength(100)
			.IsRequired();

		builder.Property(u => u.TelegramId)
			.IsRequired(false);

		builder.Property(u => u.TelegramUsername)
			.HasMaxLength(32)
			.IsRequired(false);

		builder.Property(u => u.PhoneNumberLinked)
			.HasMaxLength(20)
			.IsRequired(false);

		builder.Property(u => u.LanguageCode)
			.HasMaxLength(10)
			.IsRequired()
			.HasDefaultValue("en");

		builder.Property(u => u.IsActive)
			.IsRequired()
			.HasDefaultValue(true);

		builder.Property(u => u.Source)
			.IsRequired()
			.HasDefaultValue(UserSource.Telegram);

		builder.Property(u => u.LastActivityAt)
			.IsRequired(false);

		builder.Property(u => u.CreatedAt)
			.IsRequired();

		builder.Property(u => u.UpdatedAt)
			.IsRequired();

		builder.Property(u => u.IsDeleted)
			.IsRequired()
			.HasDefaultValue(false);

		// Indices
		builder.HasIndex(u => u.Email)
			.IsUnique();

		builder.HasIndex(u => u.TelegramId)
			.IsUnique()
			.HasFilter($"\"{nameof(UserDS.TelegramId)}\" IS NOT NULL");

		builder.HasIndex(u => u.Source);
		builder.HasIndex(u => u.IsActive);
		builder.HasIndex(u => u.CreatedAt);
		builder.HasIndex(u => u.LastActivityAt);

		// Navigation
		builder.HasMany(u => u.UserRoles)
			.WithOne()
			.HasForeignKey(ur => ur.UserId)
			.OnDelete(DeleteBehavior.Cascade);
	}
}
