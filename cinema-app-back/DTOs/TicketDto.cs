using System;

namespace cinema_app_back.DTOs
{
    public class TicketDto
    {
        public int Id { get; set; }
        public int ShowtimeId { get; set; }
        public string UserId { get; set; }
        public int SeatNumber { get; set; }
        public decimal Price { get; set; }
        public DateTime PurchaseDate { get; set; }
    }
} 