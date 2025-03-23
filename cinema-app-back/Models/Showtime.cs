using System.Text.Json.Serialization;

namespace cinema_app_back.Models
{
    public class Showtime
    {
        public int Id { get; set; }
        public DateTime StartTime { get; set; } // e.g., "2021-06-01T14:00:00"
        public DateTime EndTime { get; set; } // e.g., "2021-06-01T16:00:00"
        public int MovieId { get; set; } // Foreign key
        public decimal Price { get; set; } // Ціна квитка
        //public int HallId { get; set; } // Foreign key

        //// Навігаційні властивості з JsonIgnore для запобігання циклічних посилань
        //[JsonIgnore]
        //public virtual Hall? Hall { get; set; } // Navigation property
        [JsonIgnore]
        public virtual Movie? Movie { get; set; } // Navigation property
        //[JsonIgnore]
        //public virtual Cinema? Cinema { get; set; } // Navigation property
        
        // Можна додати колекцію квитків, якщо потрібно
        [JsonIgnore]
        public virtual ICollection<Ticket>? Tickets { get; set; }
    }
}
