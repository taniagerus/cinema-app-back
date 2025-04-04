using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace cinema_app_back.Models
{
    public class Reserve
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ShowtimeId { get; set; }

        [Required]
        public string UserId { get; set; }

        [Required]
        public int SeatId { get; set; }

        // Navigation properties
        [ForeignKey("ShowtimeId")]
        public virtual Showtime Showtime { get; set; }

        [ForeignKey("UserId")]
        public virtual User User { get; set; }

        [ForeignKey("SeatId")]
        public virtual Seat Seat { get; set; }

        public virtual Payment Payment { get; set; }
        public virtual Ticket Ticket { get; set; }
    }
}
