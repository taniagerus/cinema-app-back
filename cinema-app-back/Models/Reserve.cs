namespace cinema_app_back.Models
{
    public class Reserve
    {
        public int Id { get; set; }
        public int ShowtimeId { get; set; }
        public Showtime Showtime { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
public int SeatId { get; set; }
        public Seat Seat { get; set; }
    }
}
