using Microsoft.EntityFrameworkCore;
using DigiStore.TgBot.Domain;

namespace DigiStore.TgBot.Infrastructure.Data;

// add-migration Init -c TgBotDbContext -s DigiStore.TgBot.Web
// update-database -Context TgBotDbContext -s DigiStore.TgBot.Web

public class TgBotDbContext : DbContext
{
    public TgBotDbContext(DbContextOptions<TgBotDbContext> options) : base(options)
    {
    }

    public DbSet<TgUser> TelegramUsers { get; set; } = null!;
    public DbSet<TgUserSession> TelegramSessions { get; set; } = null!;
    public DbSet<CommandHistory> CommandHistories { get; set; } = null!;
    public DbSet<Localization> Localizations { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

		// Все таблицы по умолчанию в схеме "business"
		modelBuilder.HasDefaultSchema("TgBot");

		modelBuilder.Entity<TgUser>(eb =>
        {
            eb.ToTable("users");

			eb.HasKey(e => e.Id);

            eb.HasIndex(e => e.TelegramId).IsUnique();
            eb.Property(e => e.CreatedAt).HasDefaultValueSql("now()");
            eb.Property(e => e.UpdatedAt).HasDefaultValueSql("now()");
        });

        modelBuilder.Entity<TgUserSession>(eb =>
        {
            eb.ToTable("sessions");

			eb.HasKey(e => e.Id);

            eb.HasIndex(e => e.TelegramId);
			eb.ComplexProperty(x => x.CachedProfile, builder =>
			{
				builder.ToJson(); // JSONB
				builder.IsRequired(false); // Nullable

				// Properties внутри Complex Type
				builder.Property(x => x.Roles); // List<string> → JSONB массив
				builder.Property(x => x.Balance).HasPrecision(18, 2);
				builder.Property(x => x.Email).HasMaxLength(100);
				builder.Property(x => x.FirstName).HasMaxLength(50);
				builder.Property(x => x.LastName).HasMaxLength(50);
				builder.Property(x => x.Username).HasMaxLength(50);
			});
			eb.Property(e => e.CreatedAt).HasDefaultValueSql("now()");
            eb.Property(e => e.LastActivity).HasDefaultValueSql("now()");
        });

        modelBuilder.Entity<CommandHistory>(eb =>
        {
            eb.ToTable("command_histories");

			eb.HasKey(e => e.Id);

            eb.HasIndex(e => e.TelegramId);
            eb.Property(e => e.Timestamp).HasDefaultValueSql("now()");
        });

        modelBuilder.Entity<Localization>(eb =>
        {
            eb.ToTable("localizations");

            eb.HasKey(e => e.Key);

            eb.Property(e => e.Key).HasMaxLength(200);
            eb.Property(e => e.CreatedAt).HasDefaultValueSql("now()");
            eb.Property(e => e.UpdatedAt).HasDefaultValueSql("now()");
        });
    }
}
