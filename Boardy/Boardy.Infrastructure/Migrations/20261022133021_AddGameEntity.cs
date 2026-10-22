using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Boardy.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddGameEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "GameId",
                table: "ScoreTemplates",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Games",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    MinPlayers = table.Column<int>(type: "integer", nullable: false),
                    MaxPlayers = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Games", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ScoreTemplates_GameId",
                table: "ScoreTemplates",
                column: "GameId");

            migrationBuilder.AddForeignKey(
                name: "FK_ScoreTemplates_Games_GameId",
                table: "ScoreTemplates",
                column: "GameId",
                principalTable: "Games",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ScoreTemplates_Games_GameId",
                table: "ScoreTemplates");

            migrationBuilder.DropTable(
                name: "Games");

            migrationBuilder.DropIndex(
                name: "IX_ScoreTemplates_GameId",
                table: "ScoreTemplates");

            migrationBuilder.DropColumn(
                name: "GameId",
                table: "ScoreTemplates");
        }
    }
}
