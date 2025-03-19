namespace cinema_app_back.Models
{
    public class Seat
    {
        public int Id { get; set; }
        public int HallId { get; set; } // Foreign key
        public virtual Hall Hall { get; set; } // Navigation property
        public string Number { get; set; } // e.g., "A1"
        public bool IsReserved { get; set; }
    }
}
