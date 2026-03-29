using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace tracksByPopularity.Migrations
{
    /// <inheritdoc />
    public partial class AddPlaylistSnapshots : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PlaylistSnapshots",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    UserId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    SpotifyUserId = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PlaylistId = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PlaylistName = table.Column<string>(type: "varchar(512)", maxLength: 512, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    OperationType = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TrackCount = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6)")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlaylistSnapshots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlaylistSnapshots_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "SnapshotTracks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    SnapshotId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    TrackUri = table.Column<string>(type: "varchar(512)", maxLength: 512, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    OrderIndex = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SnapshotTracks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SnapshotTracks_PlaylistSnapshots_SnapshotId",
                        column: x => x.SnapshotId,
                        principalTable: "PlaylistSnapshots",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_PlaylistSnapshots_CreatedAt",
                table: "PlaylistSnapshots",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_PlaylistSnapshots_SpotifyUserId",
                table: "PlaylistSnapshots",
                column: "SpotifyUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PlaylistSnapshots_UserId",
                table: "PlaylistSnapshots",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_SnapshotTracks_SnapshotId",
                table: "SnapshotTracks",
                column: "SnapshotId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SnapshotTracks");

            migrationBuilder.DropTable(
                name: "PlaylistSnapshots");
        }
    }
}
