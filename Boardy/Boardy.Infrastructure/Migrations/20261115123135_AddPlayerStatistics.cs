using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Boardy.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPlayerStatistics : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PlayedGamesCount",
                table: "Players",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PlayedMatchesCount",
                table: "Players",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "WinsCount",
                table: "Players",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PlayedGamesCount",
                table: "Players");

            migrationBuilder.DropColumn(
                name: "PlayedMatchesCount",
                table: "Players");

            migrationBuilder.DropColumn(
                name: "WinsCount",
                table: "Players");
        }
    }
}
