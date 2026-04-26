using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pricing.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCourtRateOverrides : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CourtRateOverrides",
                schema: "pricing",
                columns: table => new
                {
                    CourtId = table.Column<Guid>(type: "uuid", nullable: false),
                    TariffId = table.Column<Guid>(type: "uuid", nullable: false),
                    HourlyRate = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CourtRateOverrides", x => new { x.TariffId, x.CourtId });
                    table.ForeignKey(
                        name: "FK_CourtRateOverrides_Tariffs_TariffId",
                        column: x => x.TariffId,
                        principalSchema: "pricing",
                        principalTable: "Tariffs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CourtRateOverrides",
                schema: "pricing");
        }
    }
}
