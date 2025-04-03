using System.ComponentModel.DataAnnotations;

namespace cinema_app_back.Models
{
    public class Hall
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(50)]
        public string Name { get; set; }
        
        [Required]
        [Range(1, 100)]
        public int Rows { get; set; }
        
        [Required]
        [Range(1, 100)]
        public int SeatsPerRow { get; set; }
        
        [Required]
        public int CinemaId { get; set; }
        
        public virtual Cinema? Cinema { get; set; }
        public virtual ICollection<Seat> Seats { get; set; } = new List<Seat>();
        public virtual ICollection<Showtime> Showtimes { get; set; } = new List<Showtime>();
    }
}
