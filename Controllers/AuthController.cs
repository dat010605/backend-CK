using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniOrderAPI.Data;
using MiniOrderAPI.Models;
using MiniOrderAPI.DTOs;     // Dùng LoginDto
using MiniOrderAPI.Services; // Dùng TokenService

namespace MiniOrderAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly TokenService _tokenService;

        public AuthController(AppDbContext context, TokenService tokenService)
        {
            _context = context;
            _tokenService = tokenService;
        }

        // DTO dùng riêng cho đăng ký (để khớp với JSON từ frontend gửi lên)
        public record RegisterRequest(string Username, string FullName, string Email, string Password);

        // 1. API ĐĂNG KÝ: POST api/auth/register
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest dto)
        {
            if (await _context.Users.AnyAsync(u => u.Username == dto.Username))
                return BadRequest(new { message = "Tên đăng nhập đã tồn tại" });

            if (await _context.Users.AnyAsync(u => u.Email == dto.Email))
                return BadRequest(new { message = "Email đã tồn tại" });

            var user = new User
            {
                Username = dto.Username,
                FullName = dto.FullName,
                Email = dto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Role = "User", // Mặc định đăng ký mới là User
                
                // Gán giá trị rỗng để tránh lỗi nếu chưa sửa Model
                Address = "", 
                Phone = ""
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Đăng ký thành công" });
        }

        // 2. API ĐĂNG NHẬP: POST api/auth/login
        // Đã sửa để khớp với đường dẫn Front-end gọi
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == dto.Username);

            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            {
                return Unauthorized(new { message = "Sai tên đăng nhập hoặc mật khẩu" });
            }

            // Tạo token
            var token = _tokenService.CreateToken(user);

            // QUAN TRỌNG: Trả về cả Token và Role để Front-end phân quyền
            return Ok(new 
            { 
                token = token, 
                role = user.Role 
            });
        }
    }
}