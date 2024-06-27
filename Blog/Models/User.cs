using Microsoft.AspNetCore.Identity;

namespace Blog.Models
{
    public class User : IdentityUser
    {
        public int PublicationCount { get; set; }
        public string? Name { get; set; }
    }
}
