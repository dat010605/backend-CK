using Microsoft.EntityFrameworkCore;
using MiniOrderAPI.Models;
using BCrypt.Net;

namespace MiniOrderAPI.Data
{
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
            // Kiểm tra xem đã có user nào chưa
            if (!context.Users.Any())
            {
                var admin = new User
                {
                    Username = "admin",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456"),
                    Role = "Admin",
                    // --- BỔ SUNG DỮ LIỆU BẮT BUỘC ĐỂ TRÁNH LỖI ---
                    Email = "admin@miniorder.com",
                    FullName = "Quản trị viên",
                    Address = "Hệ thống", 
                    Phone = "0000000000"
                };

                var user = new User
                {
                    Username = "user",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456"),
                    Role = "User",
                    // --- BỔ SUNG DỮ LIỆU BẮT BUỘC ĐỂ TRÁNH LỖI ---
                    Email = "user@miniorder.com",
                    FullName = "Khách hàng mẫu",
                    Address = "Hà Nội",
                    Phone = "0987654321"
                };

                context.Users.AddRange(admin, user);
                context.SaveChanges();
            }
        }
    }
}