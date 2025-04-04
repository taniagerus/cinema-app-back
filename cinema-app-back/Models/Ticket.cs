using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using cinema_app_back.Models.Enums;

namespace cinema_app_back.Models
{
    public class Ticket
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ReserveId { get; set; }

        [Required]
        public int PaymentId { get; set; }

        [Required]
        public TicketStatus Status { get; set; }

        // Navigation properties
        [ForeignKey("ReserveId")]
        public virtual Reserve Reserve { get; set; }

        [ForeignKey("PaymentId")]
        public virtual Payment Payment { get; set; }
    }
}
