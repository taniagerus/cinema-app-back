namespace cinema_app_back.Models
{
    public class Ticket
    {
     public int Id { get; set; }
        public virtual Reserve Reserve { get; set; }
        public int ReserveId { get; set; }
        public virtual Payment Payment { get; set; }
        public int PaymentId { get; set; }
    }
}
