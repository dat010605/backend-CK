using System.ComponentModel.DataAnnotations;

namespace MiniOrderAPI.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string FullName { get; set; }

        [Required]
        [EmailAddress]
        public string? Email { get; set; }

        public string Username { get; set; }
        
        public string PasswordHash { get; set; }
        
        public string Role { get; set; } // "Admin" hoặc "User"
        
        public string? Phone { get; set; }
        
        public string Address { get; set; }
    }
}   