using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Facilities.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddedImagesToFacilitiesAndCourts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "images",
                schema: "facilities",
                table: "facilities",
                type: "jsonb",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "images",
                schema: "facilities",
                table: "courts",
                type: "jsonb",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "images",
                schema: "facilities",
                table: "facilities");

            migrationBuilder.DropColumn(
                name: "images",
                schema: "facilities",
                table: "courts");
        }
    }
}
