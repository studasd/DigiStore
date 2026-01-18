using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DigiStore.TgBot.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Init2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "PendingTopUpChatId",
                schema: "TgBot",
                table: "sessions",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PendingTopUpMessageId",
                schema: "TgBot",
                table: "sessions",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PendingTopUpChatId",
                schema: "TgBot",
                table: "sessions");

            migrationBuilder.DropColumn(
                name: "PendingTopUpMessageId",
                schema: "TgBot",
                table: "sessions");
        }
    }
}
