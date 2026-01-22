using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DigiStore.TgBot.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "TgBot");

            migrationBuilder.CreateTable(
                name: "CommandHistories",
                schema: "TgBot",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TelegramId = table.Column<long>(type: "bigint", nullable: false),
                    Command = table.Column<string>(type: "text", nullable: true),
                    Message = table.Column<string>(type: "text", nullable: true),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommandHistories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Localizations",
                schema: "TgBot",
                columns: table => new
                {
                    Key = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Ru = table.Column<string>(type: "text", nullable: true),
                    En = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Localizations", x => x.Key);
                });

            migrationBuilder.CreateTable(
                name: "Sessions",
                schema: "TgBot",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    TelegramId = table.Column<long>(type: "bigint", nullable: false),
                    CurrentState = table.Column<string>(type: "text", nullable: false),
                    LangCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    MessageContexts = table.Column<string>(type: "jsonb", nullable: false),
                    PendingPayments = table.Column<string>(type: "jsonb", nullable: false),
                    LastActivity = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    CachedProfile = table.Column<string>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sessions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                schema: "TgBot",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    TelegramId = table.Column<long>(type: "bigint", nullable: false),
                    FirstName = table.Column<string>(type: "text", nullable: false),
                    LastName = table.Column<string>(type: "text", nullable: false),
                    Username = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.UserId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CommandHistories_TelegramId",
                schema: "TgBot",
                table: "CommandHistories",
                column: "TelegramId");

            migrationBuilder.CreateIndex(
                name: "IX_Sessions_TelegramId",
                schema: "TgBot",
                table: "Sessions",
                column: "TelegramId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_TelegramId",
                schema: "TgBot",
                table: "Users",
                column: "TelegramId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CommandHistories",
                schema: "TgBot");

            migrationBuilder.DropTable(
                name: "Localizations",
                schema: "TgBot");

            migrationBuilder.DropTable(
                name: "Sessions",
                schema: "TgBot");

            migrationBuilder.DropTable(
                name: "Users",
                schema: "TgBot");
        }
    }
}
