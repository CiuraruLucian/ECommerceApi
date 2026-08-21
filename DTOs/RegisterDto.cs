using System.ComponentModel.DataAnnotations;

namespace ECommerceApi.DTOs
{
    public class RegisterDto
    {
        [EmailAddress]
        [Required]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Username {  get; set; } = string.Empty;

        [Required]
        [MinLength(8)]
        public string Password { get; set; } = string.Empty;


    }
}
