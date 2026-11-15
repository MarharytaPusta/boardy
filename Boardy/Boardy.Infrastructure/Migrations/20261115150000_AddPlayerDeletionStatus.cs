using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Boardy.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPlayerDeletionStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Players",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Players");
        }
    }
}
