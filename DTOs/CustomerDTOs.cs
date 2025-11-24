using System.ComponentModel.DataAnnotations; // Dùng cho [EmailAddress], [Required]

namespace MiniOrderAPI.DTOs
{
    public class CreateCustomerDto
    {
        [Required]
        public string Name { get; set; }

        [Required]
        [EmailAddress(ErrorMessage = "Email không đúng định dạng")]
        public string Email { get; set; }

        public string Phone { get; set; }
        public string Address { get; set; }
    }
}