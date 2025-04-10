using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace cinema_app_back.Migrations
{
    /// <inheritdoc />
    public partial class AddedNewFieldsToReserve : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Reserves",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsPaid",
                table: "Reserves",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReservationTime",
                table: "Reserves",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Reserves");

            migrationBuilder.DropColumn(
                name: "IsPaid",
                table: "Reserves");

            migrationBuilder.DropColumn(
                name: "ReservationTime",
                table: "Reserves");
        }
    }
}
