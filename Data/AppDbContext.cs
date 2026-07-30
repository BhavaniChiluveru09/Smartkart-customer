using Microsoft.EntityFrameworkCore;
using SmartKart.Models;   // ✅ your User model

namespace SmartKart.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // ✅ Users table mapping
        public DbSet<User> Customers { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Cart> Cart { get; set; }
        public DbSet<Order> Orders { get; set; }

        public DbSet<SupportCase> SupportCases { get; set; }

    }
}