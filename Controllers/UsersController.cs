using Microsoft.AspNetCore.Mvc;
using MiniOrderAPI.Data;
using MiniOrderAPI.Models;
using MiniOrderAPI.DTOs; // <-- Cần để dùng RegisterUserDto
using BCrypt.Net; // <-- Cần để băm mật khẩu
using System.Linq; 
using Microsoft.EntityFrameworkCore; // Cần cho AnyAsync()

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
    // Sử dụng ToListAsync() cho thao tác bất đồng bộ
    var users = await _context.Users
        .Select(u => new 
        {
            u.Id,
            u.Username,
            u.Role
        })
        .ToListAsync(); // <-- Thêm await và ToListAsync()

    // Nếu không tìm thấy users nào, có thể trả về NotFound
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
    var newUser = new User
    {
        Username = request.Username,
        PasswordHash = passwordHash,
        Role = request.Role // Gán role từ DTO (mặc định là "User")
    };

    // 4. Lưu vào database
    _context.Users.Add(newUser);
    await _context.SaveChangesAsync();

    // 5. Trả về kết quả thành công
    // Tránh trả về PasswordHash
    return CreatedAtAction(nameof(GetUsers), new { id = newUser.Id }, new { newUser.Id, newUser.Username, newUser.Role });
}
        
    }
}