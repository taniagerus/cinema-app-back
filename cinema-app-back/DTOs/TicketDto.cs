using System;
using System.ComponentModel.DataAnnotations;

namespace cinema_app_back.DTOs
{
    public class TicketDto
    {
        public int Id { get; set; }
        public int ReserveId { get; set; }
        public int? PaymentId { get; set; }
        public DateTime CreatedAt { get; set; }
        public decimal Price { get; set; }
        public string Status { get; set; }

        // Navigation properties
        public ReserveDto Reserve { get; set; }
        public PaymentDto Payment { get; set; }
    }

    public class CreateTicketDto
    {
        [Required(ErrorMessage = "Reserve ID is required")]
        public int ReserveId { get; set; }
    }
} 