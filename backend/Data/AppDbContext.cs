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

        public DbSet<Cart> Carts
        {
            get
            {
                return Set<Cart>();
            }
        }

        public DbSet<CartItem> CartItems
        {
            get
            {
                return Set<CartItem>();
            }
        }

        public DbSet<Order> Orders
        {
            get
            {
                return Set<Order>();
            }
        }

        public DbSet<OrderItem> OrderItems
        {
            get
            {
                return Set<OrderItem>();
            }
        }
        
        #region Required
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
            
        }

        #endregion
        
    }
}

