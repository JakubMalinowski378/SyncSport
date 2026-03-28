using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Facilities.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialFacilities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "facilities");

            migrationBuilder.CreateTable(
                name: "facilities",
                schema: "facilities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Address = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    open_time = table.Column<TimeSpan>(type: "interval", nullable: false),
                    close_time = table.Column<TimeSpan>(type: "interval", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_facilities", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "courts",
                schema: "facilities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    SurfaceType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    FacilityId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_courts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_courts_facilities_FacilityId",
                        column: x => x.FacilityId,
                        principalSchema: "facilities",
                        principalTable: "facilities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_courts_FacilityId",
                schema: "facilities",
                table: "courts",
                column: "FacilityId");

            migrationBuilder.CreateIndex(
                name: "IX_facilities_Name",
                schema: "facilities",
                table: "facilities",
                column: "Name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "courts",
                schema: "facilities");

            migrationBuilder.DropTable(
                name: "facilities",
                schema: "facilities");
        }
    }
}
