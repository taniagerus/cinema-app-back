using System.ComponentModel.DataAnnotations;

namespace cinema_app_back.DTOs
{
    public class CardValidationRequest
    {
        [Required]
        [RegularExpression(@"^\d{16}$", ErrorMessage = "Card number must be 16 digits")]
        public string CardNumber { get; set; }

        [Required]
        [RegularExpression(@"^\d{2}/\d{2}$", ErrorMessage = "Expiry date must be in MM/YY format")]
        public string ExpiryDate { get; set; }

        [Required]
        [RegularExpression(@"^\d{3,4}$", ErrorMessage = "CVC must be 3 or 4 digits")]
        public string CVC { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 2)]
        public string CardholderName { get; set; }
    }
} 