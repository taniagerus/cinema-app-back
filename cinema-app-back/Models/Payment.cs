using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using cinema_app_back.Models.Enums;

namespace cinema_app_back.Models
{
    public class Payment
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ReserveId { get; set; }

        [Required]
        [StringLength(50)]
        public string PaymentMethod { get; set; }

        [Required]
        public DateTime PaymentDate { get; set; }

        public DateTime? RefundDate { get; set; }

        [Required]
        public PaymentStatus Status { get; set; }

        [Required]
        [StringLength(100)]
        public string TransactionId { get; set; }

        // Navigation properties
        [ForeignKey("ReserveId")]
        public virtual Reserve Reserve { get; set; }

        public virtual Ticket Ticket { get; set; }
    }
}
