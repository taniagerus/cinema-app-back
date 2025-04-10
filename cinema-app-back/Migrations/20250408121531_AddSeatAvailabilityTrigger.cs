using Microsoft.EntityFrameworkCore.Migrations;

namespace cinema_app_back.Migrations
{
    public partial class AddSeatAvailabilityTrigger : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                CREATE OR REPLACE FUNCTION check_seat_availability()
                RETURNS TRIGGER AS $$
                BEGIN
                    -- Check if seat is already reserved
                    IF EXISTS (
                        SELECT 1 
                        FROM ""Reserves"" 
                        WHERE ""ShowtimeId"" = NEW.""ShowtimeId"" 
                        AND ""SeatId"" = NEW.""SeatId"" 
                        AND ""IsActive"" = true
                        AND ""Id"" != NEW.""Id""
                    ) THEN
                        RAISE EXCEPTION 'Seat is already reserved';
                    END IF;

                    -- Check if showtime has already started
                    IF EXISTS (
                        SELECT 1 
                        FROM ""Showtimes"" 
                        WHERE ""Id"" = NEW.""ShowtimeId"" 
                        AND ""StartTime"" <= CURRENT_TIMESTAMP
                    ) THEN
                        RAISE EXCEPTION 'This showtime has already started or ended';
                    END IF;

                    RETURN NEW;
                END;
                $$ LANGUAGE plpgsql;

                DROP TRIGGER IF EXISTS tr_check_seat_availability ON ""Reserves"";

                CREATE TRIGGER tr_check_seat_availability
                BEFORE INSERT OR UPDATE ON ""Reserves""
                FOR EACH ROW
                EXECUTE FUNCTION check_seat_availability();
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                DROP TRIGGER IF EXISTS tr_check_seat_availability ON ""Reserves"";
                DROP FUNCTION IF EXISTS check_seat_availability();
            ");
        }
    }
} 