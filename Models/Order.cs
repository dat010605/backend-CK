namespace MiniOrderAPI.Models
{
    public class Order
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public DateTime OrderDate { get; set; } = DateTime.Now;
        public string Status { get; set; } = "New";
        public decimal TotalAmount { get; set; }

        // Navigation Properties
        public Customer Customer { get; set; }
        
        // QUAN TRỌNG: Bạn đang thiếu dòng này hoặc viết sai tên nó
        public ICollection<OrderDetail> OrderDetails { get; set; } // <--- BẮT BUỘC PHẢI CÓ
    }
}