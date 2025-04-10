using System.ComponentModel.DataAnnotations;

namespace cinema_app_back.DTOs
{
    public class CreatePaymentDto
    {
        [Required]
        public int ReserveId { get; set; }
        
        [Required]
        public string PaymentMethod { get; set; } = "CreditCard";
        
        public decimal Price { get; set; }
        
        // Додаткова інформація про платіж
        public CardDetailsDto? CardDetails { get; set; }
    }

    public class CardDetailsDto
    {
        public string? LastFourDigits { get; set; }
        public string? CardholderName { get; set; }
    }
} 