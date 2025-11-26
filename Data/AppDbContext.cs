using Microsoft.EntityFrameworkCore; // Quan trọng: Để dùng DbContext
using MiniOrderAPI.Models;           // Quan trọng: Để nhận diện Product, Order...
using BCrypt.Net;
namespace MiniOrderAPI.Data;


    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Product> Products { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }
        public DbSet<User> Users { get; set; }

       
}
public static class UserSeeder
{
    public static void Seed(AppDbContext context)
    {
        if (!context.Users.Any())
        {
            var admin = new User
            {
                Username = "admin",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456"),
                Role = "Admin"
            };

            var user = new User
            {
                Username = "user",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456"),
                Role = "User"
            };

            context.Users.Add(admin);
            context.Users.Add(user);
            context.SaveChanges();
        }
    }
}
