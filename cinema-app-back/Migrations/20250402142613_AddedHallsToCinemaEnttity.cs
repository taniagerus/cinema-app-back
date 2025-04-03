using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace cinema_app_back.Migrations
{
    /// <inheritdoc />
    public partial class AddedHallsToCinemaEnttity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tickets_AspNetUsers_UserId",
                table: "Tickets");

            migrationBuilder.DropForeignKey(
                name: "FK_Tickets_Showtimes_ShowtimeId",
                table: "Tickets");

            migrationBuilder.DropIndex(
                name: "IX_Tickets_UserId",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "SeatNumber",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Tickets");

            migrationBuilder.RenameColumn(
                name: "Number",
                table: "Seats",
                newName: "DisplayNumber");

            migrationBuilder.AlterColumn<int>(
                name: "ShowtimeId",
                table: "Tickets",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<int>(
                name: "PaymentId",
                table: "Tickets",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ReserveId",
                table: "Tickets",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsAvailable",
                table: "Seats",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "RowNumber",
                table: "Seats",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SeatNumber",
                table: "Seats",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Halls",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<int>(
                name: "Rows",
                table: "Halls",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SeatsPerRow",
                table: "Halls",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Cinemas",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Address",
                table: "Cinemas",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Cinemas",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Cinemas",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "Reserve",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ShowtimeId = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    UserId1 = table.Column<string>(type: "text", nullable: false),
                    SeatId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reserve", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Reserve_AspNetUsers_UserId1",
                        column: x => x.UserId1,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Reserve_Seats_SeatId",
                        column: x => x.SeatId,
                        principalTable: "Seats",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Reserve_Showtimes_ShowtimeId",
                        column: x => x.ShowtimeId,
                        principalTable: "Showtimes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_PaymentId",
                table: "Tickets",
                column: "PaymentId");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_ReserveId",
                table: "Tickets",
                column: "ReserveId");

            migrationBuilder.CreateIndex(
                name: "IX_Reserve_SeatId",
                table: "Reserve",
                column: "SeatId");

            migrationBuilder.CreateIndex(
                name: "IX_Reserve_ShowtimeId",
                table: "Reserve",
                column: "ShowtimeId");

            migrationBuilder.CreateIndex(
                name: "IX_Reserve_UserId1",
                table: "Reserve",
                column: "UserId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Tickets_Payments_PaymentId",
                table: "Tickets",
                column: "PaymentId",
                principalTable: "Payments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Tickets_Reserve_ReserveId",
                table: "Tickets",
                column: "ReserveId",
                principalTable: "Reserve",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Tickets_Showtimes_ShowtimeId",
                table: "Tickets",
                column: "ShowtimeId",
                principalTable: "Showtimes",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tickets_Payments_PaymentId",
                table: "Tickets");

            migrationBuilder.DropForeignKey(
                name: "FK_Tickets_Reserve_ReserveId",
                table: "Tickets");

            migrationBuilder.DropForeignKey(
                name: "FK_Tickets_Showtimes_ShowtimeId",
                table: "Tickets");

            migrationBuilder.DropTable(
                name: "Reserve");

            migrationBuilder.DropIndex(
                name: "IX_Tickets_PaymentId",
                table: "Tickets");

            migrationBuilder.DropIndex(
                name: "IX_Tickets_ReserveId",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "PaymentId",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "ReserveId",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "IsAvailable",
                table: "Seats");

            migrationBuilder.DropColumn(
                name: "RowNumber",
                table: "Seats");

            migrationBuilder.DropColumn(
                name: "SeatNumber",
                table: "Seats");

            migrationBuilder.DropColumn(
                name: "Rows",
                table: "Halls");

            migrationBuilder.DropColumn(
                name: "SeatsPerRow",
                table: "Halls");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Cinemas");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "Cinemas");

            migrationBuilder.RenameColumn(
                name: "DisplayNumber",
                table: "Seats",
                newName: "Number");

            migrationBuilder.AlterColumn<int>(
                name: "ShowtimeId",
                table: "Tickets",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SeatNumber",
                table: "Tickets",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Tickets",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "Tickets",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Halls",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Cinemas",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "Address",
                table: "Cinemas",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200);

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_UserId",
                table: "Tickets",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Tickets_AspNetUsers_UserId",
                table: "Tickets",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Tickets_Showtimes_ShowtimeId",
                table: "Tickets",
                column: "ShowtimeId",
                principalTable: "Showtimes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
