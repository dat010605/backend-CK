using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniOrderAPI.Data;    // <-- CHO AppDbContext
using MiniOrderAPI.Models;  // <-- nếu bạn dùng model User
using Microsoft.AspNetCore.Authorization;
using MiniOrderAPI.DTOs;
using MiniOrderAPI.Services;

namespace MiniOrderAPI.Data;
[Route("api/[controller]")]
[ApiController]
public class LoginController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly TokenService _tokenService;

    public LoginController(AppDbContext context, TokenService tokenService)
    {
        _context = context;
        _tokenService = tokenService;
    }

    [HttpPost]
    public async Task<IActionResult> Login(LoginDto dto)
    {
        var user = await _context.Users.FirstOrDefaultAsync(x => x.Username == dto.Username);

        if (user == null)
            return Unauthorized("User not found");

        if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            return Unauthorized("Wrong password");

        string token = _tokenService.CreateToken(user);
        string role = user.Role;

        return Ok(new { token });
    }


}

