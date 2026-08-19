using ECommerceApi.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Security.Cryptography.X509Certificates;

namespace ECommerceApi.Data
{
    public class AppDbContext : IdentityDbContext<User>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Product> Products
        {
            get
            {
                return Set<Product>();
            }
        }
        
        #region Required
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);


            modelBuilder.Entity<Product>()
            .Property(entity => entity.Name)
            .IsRequired()
            .HasMaxLength(100);

            modelBuilder.Entity<Product>()
             .Property(entity => entity.NormalizedName)
             .IsRequired()
             .HasMaxLength(100);

            modelBuilder.Entity<Product>()
             .HasIndex(entity => entity.NormalizedName)
             .IsUnique();

            modelBuilder.Entity<Product>()
              .Property(entity => entity.Description)
              .HasMaxLength(500);

            modelBuilder.Entity<Product>()
                .Property(entity => entity.Price)
                .IsRequired()
                .HasColumnType("decimal(18,2)");
            modelBuilder.Entity<Product>()
                 .Property(entity => entity.Stock)
                 .IsRequired();
        }

        #endregion
        
    }
}
