using System.ComponentModel.DataAnnotations;

namespace cinema_app_back.DTOs
{
    public class ReserveDto
    {
        public int Id { get; set; }
        public int ShowtimeId { get; set; }
        public string UserId { get; set; }
        public int SeatId { get; set; }
        public ShowtimeDto Showtime { get; set; }
        public UserDto User { get; set; }
        public SeatDto Seat { get; set; }
    }

    public class CreateReserveDto
    {
        [Required(ErrorMessage = "ShowtimeId is required")]
        public int ShowtimeId { get; set; }

        [Required(ErrorMessage = "UserId is required")]
        public string UserId { get; set; }

        [Required(ErrorMessage = "SeatId is required")]
        public int SeatId { get; set; }
    }
}
