using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SyncChat.API.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddMediaIdMessageTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "MediaId",
                table: "Messages",
                type: "uuid",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MediaId",
                table: "Messages");
        }
    }
}
