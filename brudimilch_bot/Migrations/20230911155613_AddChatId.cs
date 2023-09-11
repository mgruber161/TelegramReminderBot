using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace brudimilch_bot.Migrations
{
    /// <inheritdoc />
    public partial class AddChatId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "ChatId",
                table: "Reminders",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0L);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ChatId",
                table: "Reminders");
        }
    }
}
