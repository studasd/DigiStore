using DigiStore.Enums;
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
			entity.Property(t => t.Type).HasConversion<string>().IsRequired();
			entity.Property(t => t.Status).HasConversion<string>().IsRequired().HasDefaultValue(TransactionStatuses.Pending);
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


		// Payment configuration
		modelBuilder.Entity<PaymentDS>(entity =>
		{
			entity.ToTable("Payments");
			entity.HasKey(p => p.Id);
			entity.Property(p => p.WalletId).IsRequired();
			entity.Property(p => p.UserId).IsRequired();
			entity.Property(p => p.Aggregator).HasConversion<string>().IsRequired();
			entity.Property(p => p.AggregatorPaymentId).HasMaxLength(100);
			entity.Property(p => p.Amount).HasPrecision(18, 2).IsRequired();
			entity.Property(p => p.Currency).HasConversion<string>().IsRequired();
			entity.Property(p => p.Status).IsRequired().HasConversion<string>().HasDefaultValue(PaymentStatus.Pending);
			entity.Property(p => p.Description).HasMaxLength(500);
			entity.Property(p => p.PaymentMethod).HasMaxLength(50);
			entity.Property(p => p.ReturnUrl).HasMaxLength(500);
			entity.Property(p => p.ErrorMessage).HasMaxLength(1000);
			entity.Property(p => p.TransactionId);
			entity.Property(p => p.CreatedAt).IsRequired();
			entity.Property(p => p.UpdatedAt).IsRequired();
			entity.Property(p => p.ConfirmedAt);

			entity.HasIndex(p => p.WalletId);
			entity.HasIndex(p => p.UserId);
			entity.HasIndex(p => p.Status);
			entity.HasIndex(p => p.CreatedAt);
			entity.HasIndex(p => p.AggregatorPaymentId);
			entity.HasIndex(p => p.RecurringPaymentId);

			entity.HasOne(p => p.Wallet)
				.WithMany()
				.HasForeignKey(p => p.WalletId)
				.OnDelete(DeleteBehavior.Cascade);

			entity.HasOne(p => p.RecurringPayment)
				.WithMany(r => r.Payments)
				.HasForeignKey(p => p.RecurringPaymentId)
				.OnDelete(DeleteBehavior.SetNull);
		});

		// Withdrawal configuration
		modelBuilder.Entity<WithdrawalDS>(entity =>
		{
			entity.ToTable("Withdrawals");
			entity.HasKey(wd => wd.Id);
			entity.Property(wd => wd.WalletId).IsRequired();
			entity.Property(wd => wd.UserId).IsRequired();
			entity.Property(wd => wd.Aggregator).IsRequired().HasConversion<string>();
			entity.Property(wd => wd.AggregatorWithdrawalId).HasMaxLength(100);
			entity.Property(wd => wd.RequestedAmount).HasPrecision(18, 2).IsRequired();
			entity.Property(wd => wd.Commission).HasPrecision(18, 2).IsRequired();
			entity.Property(wd => wd.ActualAmount).HasPrecision(18, 2).IsRequired();
			entity.Property(wd => wd.Currency).HasConversion<string>().IsRequired();
			entity.Property(wd => wd.Status).IsRequired().HasDefaultValue(WithdrawalStatus.Pending);
			entity.Property(wd => wd.Description).HasMaxLength(500);
			entity.Property(wd => wd.CardMask).HasMaxLength(200);
			entity.Property(wd => wd.ErrorMessage).HasMaxLength(1000);
			entity.Property(wd => wd.TransactionId);
			entity.Property(wd => wd.CreatedAt).IsRequired();
			entity.Property(wd => wd.UpdatedAt).IsRequired();
			entity.Property(wd => wd.CompletedAt);

			entity.HasIndex(wd => wd.WalletId);
			entity.HasIndex(wd => wd.UserId);
			entity.HasIndex(wd => wd.Status);
			entity.HasIndex(wd => wd.CreatedAt);
			entity.HasIndex(wd => wd.AggregatorWithdrawalId);

			entity.HasOne(wd => wd.Wallet)
				.WithMany()
				.HasForeignKey(wd => wd.WalletId)
				.OnDelete(DeleteBehavior.Cascade);
		});

		// Recurring payment configuration
		modelBuilder.Entity<PaymentRecurringDS>(entity =>
		{
			entity.ToTable("PaymentRecurrings");
			entity.HasKey(r => r.Id);
			entity.Property(r => r.WalletId).IsRequired();
			entity.Property(r => r.UserId).IsRequired();
			entity.Property(r => r.Aggregator).IsRequired().HasConversion<string>();
			entity.Property(r => r.AggregatorRecurringId).HasMaxLength(100);
			entity.Property(r => r.Amount).HasPrecision(18, 2).IsRequired();
			entity.Property(r => r.Currency).HasConversion<string>().IsRequired();
			entity.Property(r => r.IntervalDays).IsRequired();
			entity.Property(r => r.Status).IsRequired().HasDefaultValue(PaymentRecurringStatus.Active);
			entity.Property(r => r.Description).HasMaxLength(500);
			entity.Property(r => r.PaymentInstrumentId).HasMaxLength(200);
			entity.Property(r => r.SuccessfulPayments).IsRequired();
			entity.Property(r => r.FailedPayments).IsRequired();
			entity.Property(r => r.NextPaymentDate).IsRequired();
			entity.Property(r => r.LastPaymentDate);
			entity.Property(r => r.CreatedAt).IsRequired();
			entity.Property(r => r.UpdatedAt).IsRequired();
			entity.Property(r => r.CancelledAt);

			entity.HasIndex(r => r.WalletId);
			entity.HasIndex(r => r.UserId);
			entity.HasIndex(r => r.Status);
			entity.HasIndex(r => r.NextPaymentDate);
			entity.HasIndex(r => r.AggregatorRecurringId);

			entity.HasOne(r => r.Wallet)
				.WithMany()
				.HasForeignKey(r => r.WalletId)
				.OnDelete(DeleteBehavior.Cascade);

			entity.HasMany(r => r.Payments)
				.WithOne(p => p.RecurringPayment)
				.HasForeignKey(p => p.RecurringPaymentId)
				.OnDelete(DeleteBehavior.Cascade);
		});
	}
}