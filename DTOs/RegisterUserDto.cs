namespace MiniOrderAPI.DTOs;

public class RegisterUserDto
{
    
    public string Username { get; set; } = string.Empty; 
    public string Password { get; set; } = string.Empty; 
    
    // Role mặc định là "User" nếu client không gửi lên
    public string Role { get; set; } = "User";
}