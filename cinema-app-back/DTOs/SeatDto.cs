using System.Text.Json.Serialization;
using cinema_app_back.Models;
namespace cinema_app_back.DTOs
 
{
    public class SeatDto
    {
       
        public virtual Hall Hall { get; set; } // Navigation property
        public string Number { get; set; } // e.g., "A1"
        public bool IsReserved { get; set; }
    }
}
