using Microsoft.EntityFrameworkCore;
using DigiStore.TgBot.Domain;
using System.Text.Json;

namespace DigiStore.TgBot.Infrastructure.Postgres.Data;

// add-migration Init -c TgBotDbContext -s DigiStore.TgBot.Web
// update-database -Context TgBotDbContext -s DigiStore.TgBot.Web

public class TgBotDbContext : DbContext
{
    public TgBotDbContext(DbContextOptions<TgBotDbContext> options) : base(options)
    {
    }

    public DbSet<TgUser> TelegramUsers { get; set; } = null!;
    public DbSet<TgSession> TelegramSessions { get; set; } = null!;
    public DbSet<CommandHistory> CommandHistories { get; set; } = null!;
    public DbSet<Localization> Localizations { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

		// Все таблицы по умолчанию в схеме "business"
		modelBuilder.HasDefaultSchema("TgBot");

		modelBuilder.Entity<TgUser>(eb =>
        {
            eb.ToTable("Users");

			eb.HasKey(e => e.UserId);

            eb.HasIndex(e => e.TelegramId).IsUnique();
            eb.Property(e => e.CreatedAt).HasDefaultValueSql("now()");
			eb.Property(e => e.UpdatedAt).HasDefaultValueSql("now()");
        });

        modelBuilder.Entity<TgSession>(eb =>
        {
            eb.ToTable("Sessions");

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
				builder.Property(x => x.LangCode).HasConversion<string>().HasMaxLength(10);
			});
			eb.Property(e => e.LangCode).HasConversion<string>().HasMaxLength(10);

			// Store pending payments map as JSON (jsonb)
			eb.Property(e => e.PendingPayments)
				.HasColumnType("jsonb")
				.HasConversion(
					v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
					v => string.IsNullOrWhiteSpace(v)
						? new Dictionary<Guid, Domain.ValueObjects.PendingPaymentMessageVO>()
						: (JsonSerializer.Deserialize<Dictionary<Guid, Domain.ValueObjects.PendingPaymentMessageVO>>(v, (JsonSerializerOptions?)null)
							?? new Dictionary<Guid, Domain.ValueObjects.PendingPaymentMessageVO>()));
			eb.Property(e => e.CreatedAt).HasDefaultValueSql("now()");
			eb.Property(e => e.LastActivity).HasDefaultValueSql("now()");

            // Store per-message contexts as JSON (jsonb)
            eb.Property(e => e.MessageContexts)
                .HasColumnType("jsonb")
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => string.IsNullOrWhiteSpace(v)
                        ? new Dictionary<string, Domain.ValueObjects.MessageContextVO>()
                        : (JsonSerializer.Deserialize<Dictionary<string, Domain.ValueObjects.MessageContextVO>>(v, (JsonSerializerOptions?)null)
                            ?? new Dictionary<string, Domain.ValueObjects.MessageContextVO>()));

		});

        modelBuilder.Entity<CommandHistory>(eb =>
        {
            eb.ToTable("CommandHistories");

			eb.HasKey(e => e.Id);

            eb.HasIndex(e => e.TelegramId);
            eb.Property(e => e.Timestamp).HasDefaultValueSql("now()");
            eb.Property(e => e.Command).IsRequired(false);
            eb.Property(e => e.Message).IsRequired(false);
		});

        modelBuilder.Entity<Localization>(eb =>
        {
            eb.ToTable("Localizations");

            eb.HasKey(e => e.Key);

            eb.Property(e => e.Key).HasMaxLength(200);
            eb.Property(e => e.CreatedAt).HasDefaultValueSql("now()");
            eb.Property(e => e.UpdatedAt).HasDefaultValueSql("now()");
        });
    }
}
