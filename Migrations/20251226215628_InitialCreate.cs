using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SteamPlaytimeViewer.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Games",
                columns: table => new
                {
                    AppId = table.Column<int>(type: "INTEGER", nullable: false),
                    Title = table.Column<string>(type: "TEXT", nullable: false),
                    TotalAchievements = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Games", x => x.AppId);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    SteamId = table.Column<string>(type: "TEXT", nullable: false),
                    Nickname = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.SteamId);
                });

            migrationBuilder.CreateTable(
                name: "UserGameStats",
                columns: table => new
                {
                    UserSteamId = table.Column<string>(type: "TEXT", nullable: false),
                    GameAppId = table.Column<int>(type: "INTEGER", nullable: false),
                    UnlockedAchievements = table.Column<int>(type: "INTEGER", nullable: false),
                    PlaytimeHours = table.Column<double>(type: "REAL", nullable: false),
                    FirstPlayed = table.Column<DateTime>(type: "TEXT", nullable: true),
                    LastPlayed = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserGameStats", x => new { x.UserSteamId, x.GameAppId });
                    table.ForeignKey(
                        name: "FK_UserGameStats_Games_GameAppId",
                        column: x => x.GameAppId,
                        principalTable: "Games",
                        principalColumn: "AppId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserGameStats_Users_UserSteamId",
                        column: x => x.UserSteamId,
                        principalTable: "Users",
                        principalColumn: "SteamId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserGameStats_GameAppId",
                table: "UserGameStats",
                column: "GameAppId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserGameStats");

            migrationBuilder.DropTable(
                name: "Games");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
