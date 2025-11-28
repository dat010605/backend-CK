using Microsoft.AspNetCore.Mvc;
using MiniOrderAPI.Data;
using MiniOrderAPI.Models;
using MiniOrderAPI.DTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization; // Cần dòng này để phân quyền

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

        // 1. LẤY DANH SÁCH USER
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetUsers()
        {
            var users = await _context.Users
                .Select(u => new 
                {
                    u.Id,
                    u.Username,
                    u.FullName,
                    u.Email,
                    u.Phone,
                    u.Role,
                    u.Address
                })
                .ToListAsync();

            return Ok(users);
        }

        // 2. LẤY CHI TIẾT 1 USER (Để hiển thị lên form sửa)
        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetUserById(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound(new { message = "Không tìm thấy User" });

            return Ok(new 
            {
                user.Id,
                user.Username,
                user.FullName,
                user.Email,
                user.Phone,
                user.Role,
                user.Address
            });
        }

        // 3. TẠO USER MỚI
        [HttpPost]
        public async Task<ActionResult> RegisterUser([FromBody] RegisterUserDto request)
        {
            if (await _context.Users.AnyAsync(u => u.Username == request.Username))
                return BadRequest(new { message = "Username đã tồn tại." });

            string roleToSave = (!string.IsNullOrEmpty(request.Role) && request.Role.ToLower() == "admin") ? "Admin" : "User";

            var newUser = new User
            {
                Username = request.Username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                Role = roleToSave,
                FullName = request.Username, 
                Email = request.Username + "@system.com",
                Address = "",
                Phone = ""
            };

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Tạo User thành công" });
        }

        // ==========================================
        // 4. CHỨC NĂNG SỬA USER (PUT) - MỚI THÊM
        // ==========================================
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")] // Chỉ Admin mới được sửa
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserRequest dto)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound(new { message = "Không tìm thấy User" });

            // Cập nhật thông tin cá nhân
            user.FullName = dto.FullName;
            user.Email = dto.Email;
            user.Phone = dto.Phone;
            user.Address = dto.Address;
            
            // Cập nhật Role (Chuẩn hóa)
            if (!string.IsNullOrEmpty(dto.Role))
            {
                user.Role = dto.Role.ToLower() == "admin" ? "Admin" : "User";
            }

            // Logic đổi mật khẩu: Chỉ đổi nếu người dùng nhập mật khẩu mới
            if (!string.IsNullOrEmpty(dto.Password))
            {
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = "Cập nhật User thành công" });
        }

        // ==========================================
        // 5. CHỨC NĂNG XÓA USER (DELETE) - MỚI THÊM
        // ==========================================
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")] // Chỉ Admin mới được xóa
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound(new { message = "Không tìm thấy User" });

            // (Tùy chọn) Chặn xóa chính mình để tránh lỗi không còn ai quản trị
            // var currentUsername = User.Identity.Name;
            // if (user.Username == currentUsername) return BadRequest(new { message = "Không thể tự xóa chính mình!" });

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Đã xóa User thành công" });
        }
    }

    // Class DTO dùng riêng cho việc Update (đặt ngay trong file này cho gọn)
    public class UpdateUserRequest
    {
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public string Role { get; set; }
        public string? Password { get; set; } // Dấu ? cho phép null (không đổi pass)
    }
}