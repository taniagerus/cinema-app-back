namespace cinema_app_back.DTOs
{
    public class PaymentDto
    {
        public int Id { get; set; }
        public string CardNumber { get; set; }
        public string CardHolder { get; set; }
        public string ExpirationDate { get; set; }
        public string CVV { get; set; }
    }
}
