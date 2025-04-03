using System.ComponentModel.DataAnnotations;

namespace cinema_app_back.Models
{
    public class Cinema
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        [StringLength(200)]
        public string Address { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        public string PhoneNumber { get; set; }

        public string Email { get; set; }

        // Навігаційні властивості
        public virtual ICollection<Hall> Halls { get; set; }
    }
}
