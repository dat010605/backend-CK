    namespace MiniOrderAPI.Models
    {
    public class OrderDetail
{
    public int Id { get; set; }
    public int OrderId { get; set; } // Khóa ngoại
    public int ProductId { get; set; } // Khóa ngoại
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; } // Giá tại thời điểm bán

    // Navigation Properties
    public Order Order { get; set; }
    public Product Product { get; set; }
}
    }