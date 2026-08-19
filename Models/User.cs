using Microsoft.AspNetCore.Identity;

namespace ECommerceApi.Models
{
    public class User : IdentityUser
    {
        public Cart Cart { get; set; } = null!;
    }
}
