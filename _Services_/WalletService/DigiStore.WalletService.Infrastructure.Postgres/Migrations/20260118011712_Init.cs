using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DigiStore.WalletService.Infrastructure.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "WalletService");

            migrationBuilder.CreateTable(
                name: "Wallets",
                schema: "WalletService",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Balance = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    TotalDeposited = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    TotalWithdrawn = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Currency = table.Column<string>(type: "text", nullable: false),
                    IsFrozen = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Wallets", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PaymentRecurrings",
                schema: "WalletService",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WalletId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Aggregator = table.Column<string>(type: "text", nullable: false),
                    AggregatorRecurringId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Currency = table.Column<string>(type: "text", nullable: false),
                    IntervalDays = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    PaymentInstrumentId = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    SuccessfulPayments = table.Column<int>(type: "integer", nullable: false),
                    FailedPayments = table.Column<int>(type: "integer", nullable: false),
                    NextPaymentDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastPaymentDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CancelledAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentRecurrings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PaymentRecurrings_Wallets_WalletId",
                        column: x => x.WalletId,
                        principalSchema: "WalletService",
                        principalTable: "Wallets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Transactions",
                schema: "WalletService",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WalletId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false, defaultValue: "Pending"),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    ReferenceId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ReferenceType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    BalanceAfter = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    PaymentMethod = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Transactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Transactions_Wallets_WalletId",
                        column: x => x.WalletId,
                        principalSchema: "WalletService",
                        principalTable: "Wallets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Withdrawals",
                schema: "WalletService",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WalletId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Aggregator = table.Column<string>(type: "text", nullable: false),
                    AggregatorWithdrawalId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    RequestedAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Commission = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    ActualAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Currency = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    CardMask = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ErrorMessage = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    TransactionId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Withdrawals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Withdrawals_Wallets_WalletId",
                        column: x => x.WalletId,
                        principalSchema: "WalletService",
                        principalTable: "Wallets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Payments",
                schema: "WalletService",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WalletId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Aggregator = table.Column<string>(type: "text", nullable: false),
                    AggregatorPaymentId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Currency = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false, defaultValue: "Pending"),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    PaymentMethod = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    RecurringPaymentId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReturnUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ErrorMessage = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    TransactionId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ConfirmedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Payments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Payments_PaymentRecurrings_RecurringPaymentId",
                        column: x => x.RecurringPaymentId,
                        principalSchema: "WalletService",
                        principalTable: "PaymentRecurrings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Payments_Wallets_WalletId",
                        column: x => x.WalletId,
                        principalSchema: "WalletService",
                        principalTable: "Wallets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PaymentRecurrings_AggregatorRecurringId",
                schema: "WalletService",
                table: "PaymentRecurrings",
                column: "AggregatorRecurringId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentRecurrings_NextPaymentDate",
                schema: "WalletService",
                table: "PaymentRecurrings",
                column: "NextPaymentDate");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentRecurrings_Status",
                schema: "WalletService",
                table: "PaymentRecurrings",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentRecurrings_UserId",
                schema: "WalletService",
                table: "PaymentRecurrings",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentRecurrings_WalletId",
                schema: "WalletService",
                table: "PaymentRecurrings",
                column: "WalletId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_AggregatorPaymentId",
                schema: "WalletService",
                table: "Payments",
                column: "AggregatorPaymentId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_CreatedAt",
                schema: "WalletService",
                table: "Payments",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_RecurringPaymentId",
                schema: "WalletService",
                table: "Payments",
                column: "RecurringPaymentId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_Status",
                schema: "WalletService",
                table: "Payments",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_UserId",
                schema: "WalletService",
                table: "Payments",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_WalletId",
                schema: "WalletService",
                table: "Payments",
                column: "WalletId");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_CreatedAt",
                schema: "WalletService",
                table: "Transactions",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_ReferenceId",
                schema: "WalletService",
                table: "Transactions",
                column: "ReferenceId");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_Type",
                schema: "WalletService",
                table: "Transactions",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_UserId",
                schema: "WalletService",
                table: "Transactions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_WalletId",
                schema: "WalletService",
                table: "Transactions",
                column: "WalletId");

            migrationBuilder.CreateIndex(
                name: "IX_Wallets_CreatedAt",
                schema: "WalletService",
                table: "Wallets",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Wallets_UserId",
                schema: "WalletService",
                table: "Wallets",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Withdrawals_AggregatorWithdrawalId",
                schema: "WalletService",
                table: "Withdrawals",
                column: "AggregatorWithdrawalId");

            migrationBuilder.CreateIndex(
                name: "IX_Withdrawals_CreatedAt",
                schema: "WalletService",
                table: "Withdrawals",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Withdrawals_Status",
                schema: "WalletService",
                table: "Withdrawals",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Withdrawals_UserId",
                schema: "WalletService",
                table: "Withdrawals",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Withdrawals_WalletId",
                schema: "WalletService",
                table: "Withdrawals",
                column: "WalletId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Payments",
                schema: "WalletService");

            migrationBuilder.DropTable(
                name: "Transactions",
                schema: "WalletService");

            migrationBuilder.DropTable(
                name: "Withdrawals",
                schema: "WalletService");

            migrationBuilder.DropTable(
                name: "PaymentRecurrings",
                schema: "WalletService");

            migrationBuilder.DropTable(
                name: "Wallets",
                schema: "WalletService");
        }
    }
}
