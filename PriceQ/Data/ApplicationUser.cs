using Microsoft.AspNetCore.Identity;

namespace PriceQ.Data
{
    // Extend the default IdentityUser
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
}