namespace cinema_app_back.Models
{
    public class Hall
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int CinemaId { get; set; } // Foreign key
        public virtual Cinema Cinema { get; set; } // Navigation property
        public virtual ICollection<Showtime> Showtimes { get; set; }
    }
}
