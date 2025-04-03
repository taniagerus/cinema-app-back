using System.Text.Json.Serialization;
using cinema_app_back.Models;
using System.ComponentModel.DataAnnotations;

namespace cinema_app_back.DTOs
{
    public class SeatDto
    {
        public int Id { get; set; }
        public int RowNumber { get; set; }
        public int SeatNumber { get; set; }
        public string DisplayNumber { get; set; }
        public int HallId { get; set; }
        
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public HallDto Hall { get; set; }
        
        public bool IsReserved { get; set; }
        public bool IsAvailable { get; set; }
    }

    public class CreateSeatDto
    {
        [Required]
        public int RowNumber { get; set; }
        
        [Required]
        public int SeatNumber { get; set; }
        
        [Required]
        public string DisplayNumber { get; set; }
        
        public bool IsAvailable { get; set; } = true;
        public bool IsReserved { get; set; } = false;
    }
}
