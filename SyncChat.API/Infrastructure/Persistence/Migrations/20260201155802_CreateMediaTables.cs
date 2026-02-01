using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SyncChat.API.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class CreateMediaTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Media",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    OwnerType = table.Column<int>(type: "integer", nullable: false),
                    OwnerId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    MimeType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    SizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    StorageKey = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    State = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Media", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MediaReferences",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MediaId = table.Column<Guid>(type: "uuid", nullable: false),
                    RefType = table.Column<int>(type: "integer", nullable: false),
                    RefId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MediaReferences", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MediaReferences_Media_MediaId",
                        column: x => x.MediaId,
                        principalTable: "Media",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MediaUploadSessions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MediaId = table.Column<Guid>(type: "uuid", nullable: false),
                    ExpiresAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    MaxUploads = table.Column<int>(type: "integer", nullable: false),
                    UploadCount = table.Column<int>(type: "integer", nullable: false),
                    UsedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MediaUploadSessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MediaUploadSessions_Media_MediaId",
                        column: x => x.MediaId,
                        principalTable: "Media",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Media_OwnerType_OwnerId",
                table: "Media",
                columns: new[] { "OwnerType", "OwnerId" });

            migrationBuilder.CreateIndex(
                name: "IX_Media_StorageKey",
                table: "Media",
                column: "StorageKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Media_UserId_State",
                table: "Media",
                columns: new[] { "UserId", "State" });

            migrationBuilder.CreateIndex(
                name: "IX_MediaReferences_MediaId",
                table: "MediaReferences",
                column: "MediaId");

            migrationBuilder.CreateIndex(
                name: "IX_MediaReferences_RefType_RefId",
                table: "MediaReferences",
                columns: new[] { "RefType", "RefId" });

            migrationBuilder.CreateIndex(
                name: "IX_MediaUploadSessions_ExpiresAt",
                table: "MediaUploadSessions",
                column: "ExpiresAt");

            migrationBuilder.CreateIndex(
                name: "IX_MediaUploadSessions_MediaId",
                table: "MediaUploadSessions",
                column: "MediaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MediaReferences");

            migrationBuilder.DropTable(
                name: "MediaUploadSessions");

            migrationBuilder.DropTable(
                name: "Media");
        }
    }
}
