namespace cinema_app_back.Models
{
    public class Ticket
    {
        public int Id { get; set; }
        public int ShowtimeId { get; set; } // Foreign key
        public virtual Showtime Showtime { get; set; } // Navigation property
        public string SeatNumber { get; set; } // e.g., "A1"
        public string Status { get; set; } // e.g., "Reserved"
        public string UserId { get; set; } // Foreign key
        public virtual User User { get; set; } // Navigation property
    }
}
