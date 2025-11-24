using Microsoft.EntityFrameworkCore; // Quan trọng: Để dùng DbContext
using MiniOrderAPI.Models;           // Quan trọng: Để nhận diện Product, Order...

namespace MiniOrderAPI.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Product> Products { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }
    }
}