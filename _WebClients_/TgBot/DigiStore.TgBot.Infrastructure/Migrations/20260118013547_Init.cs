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
                name: "command_histories",
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
                    table.PrimaryKey("PK_command_histories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "localizations",
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
                    table.PrimaryKey("PK_localizations", x => x.Key);
                });

            migrationBuilder.CreateTable(
                name: "sessions",
                schema: "TgBot",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    TelegramId = table.Column<long>(type: "bigint", nullable: false),
                    CurrentState = table.Column<string>(type: "text", nullable: false),
                    LangCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    PendingTopUpAggregator = table.Column<string>(type: "text", nullable: true),
                    PendingTopUpAmount = table.Column<decimal>(type: "numeric", nullable: true),
                    LastActivity = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    CachedProfile = table.Column<string>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sessions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "users",
                schema: "TgBot",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TelegramId = table.Column<long>(type: "bigint", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: true),
                    FirstName = table.Column<string>(type: "text", nullable: false),
                    LastName = table.Column<string>(type: "text", nullable: false),
                    Username = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_command_histories_TelegramId",
                schema: "TgBot",
                table: "command_histories",
                column: "TelegramId");

            migrationBuilder.CreateIndex(
                name: "IX_sessions_TelegramId",
                schema: "TgBot",
                table: "sessions",
                column: "TelegramId");

            migrationBuilder.CreateIndex(
                name: "IX_users_TelegramId",
                schema: "TgBot",
                table: "users",
                column: "TelegramId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "command_histories",
                schema: "TgBot");

            migrationBuilder.DropTable(
                name: "localizations",
                schema: "TgBot");

            migrationBuilder.DropTable(
                name: "sessions",
                schema: "TgBot");

            migrationBuilder.DropTable(
                name: "users",
                schema: "TgBot");
        }
    }
}
