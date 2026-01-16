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

	public DbSet<Wallet> Wallets => Set<Wallet>();
	public DbSet<Transaction> Transactions => Set<Transaction>();
	
	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		base.OnModelCreating(modelBuilder);
		// Wallet configuration
		modelBuilder.Entity<Wallet>(entity =>
		{
			entity.ToTable("Wallets");
			entity.HasKey(w => w.Id);
			entity.Property(w => w.UserId).IsRequired();
			entity.Property(w => w.Balance).HasPrecision(18, 2).IsRequired();
			entity.Property(w => w.TotalDeposited).HasPrecision(18, 2).IsRequired();
			entity.Property(w => w.TotalWithdrawn).HasPrecision(18, 2).IsRequired();
			entity.Property(w => w.Currency).HasMaxLength(3).IsRequired();
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
		modelBuilder.Entity<Transaction>(entity =>
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