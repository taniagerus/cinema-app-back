namespace cinema_app_back.Models
{
    public class Cinema
    {
        public int Id { get; set; }
        public string Name { get; set; } // e.g., "Cineplex"
        public string Address { get; set; } // e.g., "123 Fake St, Springfield"
        public string PhoneNumber { get; set; } // e.g., "555-555-5555"
        public virtual ICollection<Showtime> Showtimes { get; set; } // A cinema can have many showtimes
    }
}
