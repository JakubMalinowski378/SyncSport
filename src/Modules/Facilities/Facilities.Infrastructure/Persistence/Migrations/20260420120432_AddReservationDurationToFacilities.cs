using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Facilities.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddReservationDurationToFacilities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "reservation_duration",
                schema: "facilities",
                table: "facilities",
                type: "integer",
                nullable: false,
                defaultValue: 60);

            migrationBuilder.AddColumn<int>(
                name: "override_reservation_duration",
                schema: "facilities",
                table: "courts",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "reservation_duration",
                schema: "facilities",
                table: "facilities");

            migrationBuilder.DropColumn(
                name: "override_reservation_duration",
                schema: "facilities",
                table: "courts");
        }
    }
}
