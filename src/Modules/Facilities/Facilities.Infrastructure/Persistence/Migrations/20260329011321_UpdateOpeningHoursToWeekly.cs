using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Facilities.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpdateOpeningHoursToWeekly : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "close_time",
                schema: "facilities",
                table: "facilities");

            migrationBuilder.DropColumn(
                name: "open_time",
                schema: "facilities",
                table: "facilities");

            migrationBuilder.AddColumn<string>(
                name: "weekly_opening_hours",
                schema: "facilities",
                table: "facilities",
                type: "jsonb",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "override_weekly_opening_hours",
                schema: "facilities",
                table: "courts",
                type: "jsonb",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "weekly_opening_hours",
                schema: "facilities",
                table: "facilities");

            migrationBuilder.DropColumn(
                name: "override_weekly_opening_hours",
                schema: "facilities",
                table: "courts");

            migrationBuilder.AddColumn<TimeSpan>(
                name: "close_time",
                schema: "facilities",
                table: "facilities",
                type: "interval",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));

            migrationBuilder.AddColumn<TimeSpan>(
                name: "open_time",
                schema: "facilities",
                table: "facilities",
                type: "interval",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));
        }
    }
}
