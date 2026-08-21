using System.ComponentModel.DataAnnotations;

namespace ECommerceApi.DTOs
{
    public class ProductDto
    {
        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        [Required]
        public decimal Price { get; set; }

        [Required]
        public int Stock { get; set; }



    }
}
