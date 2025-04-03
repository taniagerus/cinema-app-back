using System.ComponentModel.DataAnnotations;

namespace cinema_app_back.Models
{
    public class Seat
    {
        public int Id { get; set; }
        
        [Required]
        public int RowNumber { get; set; } // Номер ряду
        
        [Required]
        public int SeatNumber { get; set; } // Номер місця в ряду
        
        [Required]
        public string DisplayNumber { get; set; } // Відображуваний номер (наприклад, "A1")
        
        public int HallId { get; set; }
        public virtual Hall Hall { get; set; }
        
        public bool IsReserved { get; set; }
        public bool IsAvailable { get; set; } = true; // За замовчуванням місце доступне
    }
}
