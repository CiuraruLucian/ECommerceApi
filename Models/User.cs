namespace ECommerceApi.Models
{
    public class User
    {
        public int Id { get; set; }

        public string Password { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string Role { get; set; } = "Customer";

        
    }
}
