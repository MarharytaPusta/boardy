using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Boardy.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMatchCriterionResults : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MatchCriterionResults",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MatchParticipantId = table.Column<Guid>(type: "uuid", nullable: false),
                    ScoreCriterionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Score = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MatchCriterionResults", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MatchCriterionResults_MatchParticipants_MatchParticipantId",
                        column: x => x.MatchParticipantId,
                        principalTable: "MatchParticipants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MatchCriterionResults_MatchParticipantId",
                table: "MatchCriterionResults",
                column: "MatchParticipantId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MatchCriterionResults");
        }
    }
}
