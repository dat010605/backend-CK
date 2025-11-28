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

            // --- SỬA LỖI Ở ĐÂY: Logic chuẩn hóa Role ---
            // Mục đích: Nếu Swagger gửi "admin" (chữ thường) thì tự sửa thành "Admin" (chữ hoa)
            string roleToSave = request.Role;
            
            if (!string.IsNullOrEmpty(roleToSave))
            {
                if (roleToSave.ToLower() == "admin")
                {
                    roleToSave = "Admin"; // Ép về chuẩn chữ hoa đầu
                }
            }
            else
            {
                roleToSave = "User"; // Mặc định là User nếu không gửi
            }

            // 3. Tạo đối tượng User mới
            var newUser = new User
            {
                Username = request.Username,
                PasswordHash = passwordHash,
                
                // SỬA: Gán role bằng biến đã chuẩn hóa ở trên
                Role = roleToSave, 
                
                // --- BỔ SUNG CÁC TRƯỜNG BẮT BUỘC ---
                FullName = request.Username,           // Mặc định lấy username làm tên
                Email = request.Username + "@system.com", // Email giả lập
                Address = "Chưa cập nhật",             
                Phone = "0000000000"                   
            };

            // 4. Lưu vào database
            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            // 5. Trả về kết quả thành công
            return CreatedAtAction(nameof(GetUsers), new { id = newUser.Id }, new { newUser.Id, newUser.Username, newUser.Role });
        }
    }
}