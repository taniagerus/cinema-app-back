using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using cinema_app_back.Models;

namespace cinema_app_back.DTOs
{
    public class HallDto
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(50)]
        public string Name { get; set; }
        
        [Required]
        [Range(1, 100)]
        public int Rows { get; set; }
        
        [Required]
        [Range(1, 100)]
        public int SeatsPerRow { get; set; }
        
        [Required]
        public int CinemaId { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public CinemaDto Cinema { get; set; }
        
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public ICollection<SeatDto> Seats { get; set; }
        
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public ICollection<ShowtimeDto> Showtimes { get; set; }
    }

    public class CreateHallDto
    {
        [Required(ErrorMessage = "Назва залу обов'язкова")]
        [StringLength(50, ErrorMessage = "Назва залу не може перевищувати 50 символів")]
        public string Name { get; set; }
        
        [Required(ErrorMessage = "Кількість рядів обов'язкова")]
        [Range(1, 100, ErrorMessage = "Кількість рядів має бути від 1 до 100")]
        public int Rows { get; set; }
        
        [Required(ErrorMessage = "Кількість місць в ряді обов'язкова")]
        [Range(1, 100, ErrorMessage = "Кількість місць в ряді має бути від 1 до 100")]
        public int SeatsPerRow { get; set; }
        
        [Required(ErrorMessage = "ID кінотеатру обов'язковий")]
        public int CinemaId { get; set; }
    }
}
