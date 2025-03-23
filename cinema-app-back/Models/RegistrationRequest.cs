using System.ComponentModel.DataAnnotations;

namespace cinema_app_back.Models
{
    public class RegistrationRequest
    {
        [Required]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public string? Email { get; set; }

        [Required]
        public string? FirstName { get; set; }

        [Required]
        public string? LastName { get; set; }

        [Required]
        [RegularExpression(@"^[a-zA-Z0-9]+$", ErrorMessage = "Username can only contain letters and digits.")]
        public string? Username { get; set; }

        [Required]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters long.")]
        public string? Password { get; set; }

        [Required]
        public DateTime Birthday { get; set; }

        public Role Role { get; set; }
    }
}
