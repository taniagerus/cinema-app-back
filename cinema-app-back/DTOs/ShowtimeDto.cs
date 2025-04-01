using System.Text.Json.Serialization;
using cinema_app_back.Models;

namespace cinema_app_back.DTOs
{
    public class ShowtimeDto
    {
        public int Id { get; set; }
        public DateTime StartTime { get; set; } 
        public DateTime EndTime { get; set; } 
        public int MovieId { get; set; } 
        public decimal Price { get; set; } // Ціна квитка
        public int HallId { get; set; } // ID зали
        public int CinemaId { get; set; } // ID кінотеатру
      
        // Навігаційні властивості 
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public virtual Hall? Hall { get; set; } 
        
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public virtual Movie? Movie { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public virtual Cinema? Cinema { get; set; }
    }
}
