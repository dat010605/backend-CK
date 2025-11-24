using System.ComponentModel.DataAnnotations; // Dùng cho [Required], [Range]

namespace MiniOrderAPI.DTOs
{
    public class CreateOrderDto
    {
        [Required]
        public int CustomerId { get; set; }
        
        [Required]
        [MinLength(1, ErrorMessage = "Đơn hàng phải có ít nhất 1 sản phẩm")]
        public List<CreateOrderDetailDto> Items { get; set; }
    }

    public class CreateOrderDetailDto
    {
        [Required]
        public int ProductId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Số lượng mua phải lớn hơn 0")]
        public int Quantity { get; set; }
    }
}