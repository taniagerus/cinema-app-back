using System.Text.Json.Serialization;
using cinema_app_back.Models;

namespace cinema_app_back.DTOs
{
    public class HallDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int CinemaId { get; set; } // Foreign key
        
        // Навігаційні властивості 
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public virtual Cinema? Cinema { get; set; } // Navigation property
        public virtual ICollection<Showtime> Showtimes { get; set; }
    }
}
