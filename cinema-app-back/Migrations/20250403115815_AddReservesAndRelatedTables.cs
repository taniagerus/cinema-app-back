using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace cinema_app_back.Migrations
{
    /// <inheritdoc />
    public partial class AddReservesAndRelatedTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Payments_AspNetUsers_UserId",
                table: "Payments");

            migrationBuilder.DropForeignKey(
                name: "FK_Reserve_AspNetUsers_UserId1",
                table: "Reserve");

            migrationBuilder.DropForeignKey(
                name: "FK_Reserve_Seats_SeatId",
                table: "Reserve");

            migrationBuilder.DropForeignKey(
                name: "FK_Reserve_Showtimes_ShowtimeId",
                table: "Reserve");

            migrationBuilder.DropForeignKey(
                name: "FK_Tickets_Reserve_ReserveId",
                table: "Tickets");

            migrationBuilder.DropIndex(
                name: "IX_Tickets_PaymentId",
                table: "Tickets");

            migrationBuilder.DropIndex(
                name: "IX_Tickets_ReserveId",
                table: "Tickets");

            migrationBuilder.DropIndex(
                name: "IX_Payments_UserId",
                table: "Payments");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Reserve",
                table: "Reserve");

            migrationBuilder.DropIndex(
                name: "IX_Reserve_UserId1",
                table: "Reserve");

            migrationBuilder.DropColumn(
                name: "BillingAddress",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "CVV",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "CardNumber",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "ExpirationDate",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "UserId1",
                table: "Reserve");

            migrationBuilder.RenameTable(
                name: "Reserve",
                newName: "Reserves");

            migrationBuilder.RenameIndex(
                name: "IX_Reserve_ShowtimeId",
                table: "Reserves",
                newName: "IX_Reserves_ShowtimeId");

            migrationBuilder.RenameIndex(
                name: "IX_Reserve_SeatId",
                table: "Reserves",
                newName: "IX_Reserves_SeatId");

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Tickets",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "PaymentMethod",
                table: "Payments",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<DateTime>(
                name: "PaymentDate",
                table: "Payments",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "RefundDate",
                table: "Payments",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ReserveId",
                table: "Payments",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Payments",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "TransactionId",
                table: "Payments",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "Reserves",
                type: "text",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Reserves",
                table: "Reserves",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_PaymentId",
                table: "Tickets",
                column: "PaymentId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_ReserveId",
                table: "Tickets",
                column: "ReserveId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Payments_ReserveId",
                table: "Payments",
                column: "ReserveId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Reserves_UserId",
                table: "Reserves",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_Reserves_ReserveId",
                table: "Payments",
                column: "ReserveId",
                principalTable: "Reserves",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Reserves_AspNetUsers_UserId",
                table: "Reserves",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Reserves_Seats_SeatId",
                table: "Reserves",
                column: "SeatId",
                principalTable: "Seats",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Reserves_Showtimes_ShowtimeId",
                table: "Reserves",
                column: "ShowtimeId",
                principalTable: "Showtimes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Tickets_Reserves_ReserveId",
                table: "Tickets",
                column: "ReserveId",
                principalTable: "Reserves",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Payments_Reserves_ReserveId",
                table: "Payments");

            migrationBuilder.DropForeignKey(
                name: "FK_Reserves_AspNetUsers_UserId",
                table: "Reserves");

            migrationBuilder.DropForeignKey(
                name: "FK_Reserves_Seats_SeatId",
                table: "Reserves");

            migrationBuilder.DropForeignKey(
                name: "FK_Reserves_Showtimes_ShowtimeId",
                table: "Reserves");

            migrationBuilder.DropForeignKey(
                name: "FK_Tickets_Reserves_ReserveId",
                table: "Tickets");

            migrationBuilder.DropIndex(
                name: "IX_Tickets_PaymentId",
                table: "Tickets");

            migrationBuilder.DropIndex(
                name: "IX_Tickets_ReserveId",
                table: "Tickets");

            migrationBuilder.DropIndex(
                name: "IX_Payments_ReserveId",
                table: "Payments");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Reserves",
                table: "Reserves");

            migrationBuilder.DropIndex(
                name: "IX_Reserves_UserId",
                table: "Reserves");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "PaymentDate",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "RefundDate",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "ReserveId",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "TransactionId",
                table: "Payments");

            migrationBuilder.RenameTable(
                name: "Reserves",
                newName: "Reserve");

            migrationBuilder.RenameIndex(
                name: "IX_Reserves_ShowtimeId",
                table: "Reserve",
                newName: "IX_Reserve_ShowtimeId");

            migrationBuilder.RenameIndex(
                name: "IX_Reserves_SeatId",
                table: "Reserve",
                newName: "IX_Reserve_SeatId");

            migrationBuilder.AlterColumn<string>(
                name: "PaymentMethod",
                table: "Payments",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AddColumn<string>(
                name: "BillingAddress",
                table: "Payments",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CVV",
                table: "Payments",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CardNumber",
                table: "Payments",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ExpirationDate",
                table: "Payments",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "Payments",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<int>(
                name: "UserId",
                table: "Reserve",
                type: "integer",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<string>(
                name: "UserId1",
                table: "Reserve",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Reserve",
                table: "Reserve",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_PaymentId",
                table: "Tickets",
                column: "PaymentId");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_ReserveId",
                table: "Tickets",
                column: "ReserveId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_UserId",
                table: "Payments",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Reserve_UserId1",
                table: "Reserve",
                column: "UserId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_AspNetUsers_UserId",
                table: "Payments",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Reserve_AspNetUsers_UserId1",
                table: "Reserve",
                column: "UserId1",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Reserve_Seats_SeatId",
                table: "Reserve",
                column: "SeatId",
                principalTable: "Seats",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Reserve_Showtimes_ShowtimeId",
                table: "Reserve",
                column: "ShowtimeId",
                principalTable: "Showtimes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Tickets_Reserve_ReserveId",
                table: "Tickets",
                column: "ReserveId",
                principalTable: "Reserve",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
