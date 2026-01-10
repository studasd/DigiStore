using Microsoft.EntityFrameworkCore;
using DigiStore.TgBot.Domain;

namespace DigiStore.TgBot.Infrastructure.Data;

// add-migration Init -c TgBotDbContext        // -s DigiStore
// update-database -Context TgBotDbContext

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
            eb.Property(e => e.Data).HasColumnType("jsonb");
            eb.Property(e => e.CachedProfile).HasColumnType("jsonb");
            eb.Property(e => e.CreatedAt).HasDefaultValueSql("now()");
            eb.Property(e => e.LastActivity).HasDefaultValueSql("now()");
        });

        modelBuilder.Entity<CommandHistory>(eb =>
        {
            eb.ToTable("command_histories");

			eb.HasKey(e => e.Id);

            eb.HasIndex(e => e.TelegramId);
            eb.Property(e => e.Payload).HasColumnType("jsonb");
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
