using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Boardy.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddScoreTemplateEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ScoreTemplates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScoreTemplates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ScoreCriteria",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    IsNegative = table.Column<bool>(type: "boolean", nullable: false),
                    ScoreTemplateId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScoreCriteria", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ScoreCriteria_ScoreTemplates_ScoreTemplateId",
                        column: x => x.ScoreTemplateId,
                        principalTable: "ScoreTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ScoreCriteria_ScoreTemplateId",
                table: "ScoreCriteria",
                column: "ScoreTemplateId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ScoreCriteria");

            migrationBuilder.DropTable(
                name: "ScoreTemplates");
        }
    }
}
