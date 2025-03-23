using Microsoft.AspNetCore.Identity;

namespace cinema_app_back.Models
{
    public class User : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public Role Role { get; set; }
        public DateTime Birthday { get; set; }
    }
}
