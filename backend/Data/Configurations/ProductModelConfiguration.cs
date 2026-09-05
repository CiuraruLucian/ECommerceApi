using ECommerceApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;


namespace ECommerceApi.Data.Configurations
{
    public class ProductModelConfiguration : IEntityTypeConfiguration<Product>
    {
        public void Configure(EntityTypeBuilder<Product> builder)
        {
            builder
                .Property(p => p.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder
                 .Property(p => p.NormalizedName)
                 .IsRequired()
                 .HasMaxLength(100);

            builder
                 .HasIndex(p => p.NormalizedName)
                 .IsUnique();

            builder
                  .Property(p => p.Description)
                  .HasMaxLength(500);

            builder
                    .Property(p => p.Price)
                    .IsRequired()
                    .HasColumnType("decimal(18,2)");
            builder
                     .Property(p => p.Stock)
                     .IsRequired();

        }
    }
}
