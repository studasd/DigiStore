using DigiStore.WalletService.Domain;
using DigiStore.WalletService.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace DigiStore.WalletService.Infrastructure.Postgres.Data;

// add-migration Init -c WalletDbContext -s DigiStore.WalletService.Web
// update-database -Context WalletDbContext -s DigiStore.WalletService.Web

public class WalletDbContext : DbContext
{
	public WalletDbContext(DbContextOptions<WalletDbContext> options) : base(options)
	{
	}

	public DbSet<WalletDS> Wallets => Set<WalletDS>();
	public DbSet<TransactionDS> Transactions => Set<TransactionDS>();


	public DbSet<PaymentDS> Payments => Set<PaymentDS>();
	public DbSet<WithdrawalDS> Withdrawals => Set<WithdrawalDS>();
	public DbSet<PaymentRecurringDS> PaymentRecurrings => Set<PaymentRecurringDS>();



	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		base.OnModelCreating(modelBuilder);

		modelBuilder.HasDefaultSchema("WalletService");

		// Wallet configuration
		modelBuilder.Entity<WalletDS>(entity =>
		{
			entity.ToTable("Wallets");
			entity.HasKey(w => w.Id);
			entity.Property(w => w.UserId).IsRequired();
			entity.Property(w => w.Balance).HasPrecision(18, 2).IsRequired();
			entity.Property(w => w.TotalDeposited).HasPrecision(18, 2).IsRequired();
			entity.Property(w => w.TotalWithdrawn).HasPrecision(18, 2).IsRequired();
			entity.Property(w => w.Currency).HasConversion<string>().IsRequired();
			entity.Property(w => w.IsFrozen).IsRequired().HasDefaultValue(false);
			entity.Property(w => w.CreatedAt).IsRequired();
			entity.Property(w => w.UpdatedAt).IsRequired();
			entity.HasIndex(w => w.UserId).IsUnique();
			
			entity.HasIndex(w => w.CreatedAt);
			entity.HasMany(w => w.Transactions)
				.WithOne(t => t.Wallet)
				.HasForeignKey(t => t.WalletId)
				.OnDelete(DeleteBehavior.Cascade);
		});

		// Transaction configuration
		modelBuilder.Entity<TransactionDS>(entity =>
		{
			entity.ToTable("Transactions");
			entity.HasKey(t => t.Id);
			entity.Property(t => t.WalletId).IsRequired();
			entity.Property(t => t.UserId).IsRequired();
			entity.Property(t => t.Amount).HasPrecision(18, 2).IsRequired();
			entity.Property(t => t.Type).IsRequired();
			entity.Property(t => t.Status).IsRequired().HasDefaultValue(TransactionStatuses.Pending);
			entity.Property(t => t.Description).HasMaxLength(500);
			entity.Property(t => t.ReferenceId).HasMaxLength(100);
			entity.Property(t => t.ReferenceType).HasMaxLength(50);
			entity.Property(t => t.BalanceAfter).HasPrecision(18, 2).IsRequired();
			entity.Property(t => t.PaymentMethod).HasMaxLength(50);
			entity.Property(t => t.CreatedAt).IsRequired();
			entity.HasIndex(t => t.WalletId);
			entity.HasIndex(t => t.UserId);
			entity.HasIndex(t => t.Type);
			entity.HasIndex(t => t.CreatedAt);
			entity.HasIndex(t => t.ReferenceId);
		});
	}
}