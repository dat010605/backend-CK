using Microsoft.AspNetCore.Mvc;
using MiniOrderAPI.Data;
using MiniOrderAPI.Models;
using MiniOrderAPI.DTOs;
using BCrypt.Net;
using System.Linq; 
using Microsoft.EntityFrameworkCore;

namespace MiniOrderAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UsersController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetUsers()
        {
            var users = await _context.Users
                .Select(u => new 
                {
                    u.Id,
                    u.Username,
                    u.Role
                })
                .ToListAsync();

            if (users == null || !users.Any())
            {
                return NotFound("Không tìm thấy người dùng nào.");
            }

            return Ok(users);
        }

        // --- POST METHOD ĐỂ TẠO TÀI KHOẢN ---
        [HttpPost]
        public async Task<ActionResult> RegisterUser([FromBody] RegisterUserDto request)
        {
            // 1. Kiểm tra username đã tồn tại chưa
            if (await _context.Users.AnyAsync(u => u.Username == request.Username))
            {
                return BadRequest("Username đã tồn tại.");
            }

            // 2. Băm mật khẩu (Hashing)
            string passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

            // 3. Tạo đối tượng User mới
            // SỬA LỖI: Thêm các trường dữ liệu mặc định để tránh lỗi "NOT NULL constraint failed"
            var newUser = new User
            {
                Username = request.Username,
                PasswordHash = passwordHash,
                Role = request.Role, // Gán role từ DTO (mặc định là "User")
                
                // --- BỔ SUNG CÁC TRƯỜNG BẮT BUỘC (QUAN TRỌNG) ---
                FullName = request.Username,           // Mặc định lấy username làm tên
                Email = request.Username + "@system.com", // Email giả lập để không bị lỗi
                Address = "Chưa cập nhật",             // Giá trị mặc định cho Address
                Phone = "0000000000"                   // Giá trị mặc định cho Phone
            };

            // 4. Lưu vào database
            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            // 5. Trả về kết quả thành công
            return CreatedAtAction(nameof(GetUsers), new { id = newUser.Id }, new { newUser.Id, newUser.Username, newUser.Role });
        }
    }
}