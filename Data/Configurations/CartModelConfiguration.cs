using ECommerceApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerceApi.Data.Configurations
{
    public class CartModelConfiguration : IEntityTypeConfiguration<Cart>
    {
        public void Configure(EntityTypeBuilder<Cart> builder)
        {
            builder
                .HasOne<User>()
                .WithOne(u => u.Cart)
                .HasForeignKey<Cart>(u => u.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
