using System.Text.Json.Serialization;
using cinema_app_back.Models;

namespace cinema_app_back.DTOs
{
    public class CinemaDto
    {
        public int Id { get; set; }
        public string Name { get; set; } // e.g., "Cineplex"
        public string Address { get; set; } // e.g., "123 Fake St, Springfield"
        public string Description { get; set; }
        public string PhoneNumber { get; set; } // e.g., "555-555-5555"
        public string Email { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public ICollection<HallDto> Halls { get; set; }
    }
}
