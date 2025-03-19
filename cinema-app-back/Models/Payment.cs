namespace cinema_app_back.Models
{
    public class Payment
    {
        public int Id { get; set; }
        public string UserId { get; set; } // Foreign key
        public virtual User User { get; set; } // Navigation property
        public string PaymentMethod { get; set; } // e.g., "Credit Card"
        public string CardNumber { get; set; } // e.g., "1234 5678 1234 5678"
        public string ExpirationDate { get; set; } // e.g., "12/23"
        public string CVV { get; set; } // e.g., "123"
        public string BillingAddress { get; set; } // e.g., "1234 Main St, Springfield, IL 62701"
    }
}
