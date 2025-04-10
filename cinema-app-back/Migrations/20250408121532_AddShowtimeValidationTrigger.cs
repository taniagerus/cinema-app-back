using Microsoft.EntityFrameworkCore.Migrations;

namespace cinema_app_back.Migrations
{
    public partial class AddShowtimeValidationTrigger : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                CREATE OR REPLACE FUNCTION check_showtime_validity()
                RETURNS TRIGGER AS $$
                BEGIN
                    -- Check if end time is after start time (considering next day)
                    IF NEW.""EndTime"" <= NEW.""StartTime"" AND 
                       EXTRACT(EPOCH FROM (NEW.""EndTime"" - NEW.""StartTime"")) > -72000 -- 20 hours in seconds
                    THEN
                        RAISE EXCEPTION 'End time must be after start time';
                    END IF;

                    -- Check if showtime overlaps with existing ones in the same hall
                    IF EXISTS (
                        SELECT 1 
                        FROM ""Showtimes"" s
                        WHERE s.""HallId"" = NEW.""HallId""
                        AND s.""Id"" != NEW.""Id""
                        AND (
                            (NEW.""StartTime"" BETWEEN s.""StartTime"" AND s.""EndTime"")
                            OR (NEW.""EndTime"" BETWEEN s.""StartTime"" AND s.""EndTime"")
                            OR (NEW.""StartTime"" <= s.""StartTime"" AND NEW.""EndTime"" >= s.""EndTime"")
                        )
                    ) THEN
                        RAISE EXCEPTION 'Showtime overlaps with existing showtime in the same hall';
                    END IF;

                    -- Check if start time is in the future for new showtimes
                    IF TG_OP = 'INSERT' AND NEW.""StartTime"" <= CURRENT_TIMESTAMP THEN
                        RAISE EXCEPTION 'Start time must be in the future';
                    END IF;

                    RETURN NEW;
                END;
                $$ LANGUAGE plpgsql;

                DROP TRIGGER IF EXISTS tr_check_showtime_validity ON ""Showtimes"";

                CREATE TRIGGER tr_check_showtime_validity
                BEFORE INSERT OR UPDATE ON ""Showtimes""
                FOR EACH ROW
                EXECUTE FUNCTION check_showtime_validity();
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                DROP TRIGGER IF EXISTS tr_check_showtime_validity ON ""Showtimes"";
                DROP FUNCTION IF EXISTS check_showtime_validity();
            ");
        }
    }
} 