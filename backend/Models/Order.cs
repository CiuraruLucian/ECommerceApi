namespace ECommerceApi.Models
{
    public class Order
    {
        public int Id { get; set; }

        public string UserId { get; set; } = string.Empty;

        public decimal Total { get; set; }

        public string? PaymentIntentId { get; set; }

        public string Status { get; set; }
        public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
    }
}
