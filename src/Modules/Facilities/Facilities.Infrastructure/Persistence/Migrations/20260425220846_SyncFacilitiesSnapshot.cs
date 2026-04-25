using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Facilities.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SyncFacilitiesSnapshot : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_facilities_Name",
                schema: "facilities",
                table: "facilities");

            migrationBuilder.AddColumn<string>(
                name: "slug",
                schema: "facilities",
                table: "facilities",
                type: "character varying(220)",
                maxLength: 220,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "slug",
                schema: "facilities",
                table: "courts",
                type: "character varying(120)",
                maxLength: 120,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_facilities_slug",
                schema: "facilities",
                table: "facilities",
                column: "slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_courts_slug",
                schema: "facilities",
                table: "courts",
                column: "slug",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_facilities_slug",
                schema: "facilities",
                table: "facilities");

            migrationBuilder.DropIndex(
                name: "IX_courts_slug",
                schema: "facilities",
                table: "courts");

            migrationBuilder.DropColumn(
                name: "slug",
                schema: "facilities",
                table: "facilities");

            migrationBuilder.DropColumn(
                name: "slug",
                schema: "facilities",
                table: "courts");

            migrationBuilder.CreateIndex(
                name: "IX_facilities_Name",
                schema: "facilities",
                table: "facilities",
                column: "Name",
                unique: true);
        }
    }
}
