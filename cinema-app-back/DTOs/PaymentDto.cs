using System;
using System.ComponentModel.DataAnnotations;
using cinema_app_back.Models.Enums;

namespace cinema_app_back.DTOs
{
    public class PaymentDto
    {
        public int Id { get; set; }
        public int ReserveId { get; set; }
        public string PaymentMethod { get; set; }
        public DateTime PaymentDate { get; set; }
        public DateTime? RefundDate { get; set; }
        public PaymentStatus Status { get; set; }
        public string TransactionId { get; set; }
        public decimal Amount { get; set; }
    }
}
