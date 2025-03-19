namespace cinema_app_back.Models
{
    public class Showtime
    {
        public int Id { get; set; }
        public DateTime StartTime { get; set; } // e.g., "2021-06-01T14:00:00"
        public DateTime EndTime { get; set; } // e.g., "2021-06-01T16:00:00"
        public int MovieId { get; set; } // Foreign key
        public int HallId { get; set; } // Foreign key
        public int CinemaId { get; set; } // Foreign key

        public virtual Hall? Hall { get; set; } // Navigation property
        public virtual Movie? Movie { get; set; } // Navigation property
        public virtual Cinema? Cinema { get; set; } // Navigation property
    }
}
