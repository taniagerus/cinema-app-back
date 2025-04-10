using System;
using System.ComponentModel.DataAnnotations;

namespace cinema_app_back.DTOs
{
    public class ReserveDto
    {
        public int Id { get; set; }
        
        [Required]
        public string UserId { get; set; } = string.Empty;
        
        public bool IsActive { get; set; }
        
        public ShowtimeDto? Showtime { get; set; }
        public SeatDto? Seat { get; set; }
        public PaymentDto? Payment { get; set; }
        public TicketDto? Ticket { get; set; }
    }

    public class CreateReserveDto
    {
        [Required]
        public int ShowtimeId { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        public int SeatId { get; set; }
    }

   
}
