using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace cinema_app_back.Migrations
{
    /// <inheritdoc />
    public partial class AddHallForeignKeyToShowtime : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Showtimes_Halls_HallId",
                table: "Showtimes");

            migrationBuilder.AlterColumn<int>(
                name: "HallId",
                table: "Showtimes",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Showtimes_Halls_HallId",
                table: "Showtimes",
                column: "HallId",
                principalTable: "Halls",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Showtimes_Halls_HallId",
                table: "Showtimes");

            migrationBuilder.AlterColumn<int>(
                name: "HallId",
                table: "Showtimes",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddForeignKey(
                name: "FK_Showtimes_Halls_HallId",
                table: "Showtimes",
                column: "HallId",
                principalTable: "Halls",
                principalColumn: "Id");
        }
    }
}
