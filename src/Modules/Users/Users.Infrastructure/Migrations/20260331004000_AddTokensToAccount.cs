using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Users.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTokensToAccount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PasswordResetToken",
                schema: "users",
                table: "Accounts",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PasswordResetTokenExpiryTime",
                schema: "users",
                table: "Accounts",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RefreshToken",
                schema: "users",
                table: "Accounts",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RefreshTokenExpiryTime",
                schema: "users",
                table: "Accounts",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PasswordResetToken",
                schema: "users",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "PasswordResetTokenExpiryTime",
                schema: "users",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "RefreshToken",
                schema: "users",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "RefreshTokenExpiryTime",
                schema: "users",
                table: "Accounts");
        }
    }
}
