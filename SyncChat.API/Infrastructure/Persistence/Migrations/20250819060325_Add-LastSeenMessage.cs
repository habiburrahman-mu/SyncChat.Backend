using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SyncChat.API.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddLastSeenMessage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "LastSeenMessageId",
                table: "ConversationMembers",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ConversationMembers_LastSeenMessageId",
                table: "ConversationMembers",
                column: "LastSeenMessageId");

            migrationBuilder.AddForeignKey(
                name: "FK_ConversationMembers_Messages_LastSeenMessageId",
                table: "ConversationMembers",
                column: "LastSeenMessageId",
                principalTable: "Messages",
                principalColumn: "MessageId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ConversationMembers_Messages_LastSeenMessageId",
                table: "ConversationMembers");

            migrationBuilder.DropIndex(
                name: "IX_ConversationMembers_LastSeenMessageId",
                table: "ConversationMembers");

            migrationBuilder.DropColumn(
                name: "LastSeenMessageId",
                table: "ConversationMembers");
        }
    }
}
