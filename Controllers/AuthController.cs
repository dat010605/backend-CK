using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniOrderAPI.Data;    // <-- CHO AppDbContext
using MiniOrderAPI.Models;  // <-- nếu bạn dùng model User
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;

namespace MiniOrderAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AuthController(AppDbContext context)
        {
            _context = context;
        }

        // DTO đăng ký - chỉnh fields nếu cần
        public record RegisterDto(string Username, string FullName, string Email, string Password);

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            if (dto is null || string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Password))
                return BadRequest(new { error = "Email và password là bắt buộc." });

            // Kiểm tra email tồn tại
            var exists = await _context.Users.AnyAsync(u => u.Email == dto.Email);
            if (exists)
                return Conflict(new { error = "Email đã được sử dụng." });

            // Tạo user mới - chỉnh property nếu model User của bạn khác tên
            var user = new User
            {
                Username = dto.Username,
                FullName = dto.FullName,
                Email = dto.Email,
                // Lưu hash mật khẩu (gói BCrypt.Net-Next đã có trong csproj)
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password)
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Đăng ký thành công." });
        }
    }
}