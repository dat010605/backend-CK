namespace MiniOrderAPI.Models
{
    public class Order
    {
        public int Id { get; set; }

        // Sửa CustomerId thành UserId
        public int UserId { get; set; } 

        public DateTime OrderDate { get; set; } = DateTime.Now;
        public string Status { get; set; } = "New";
        public decimal TotalAmount { get; set; }

        // Sửa Navigation Property trỏ về User
        public User User { get; set; }
        
        public ICollection<OrderDetail> OrderDetails { get; set; }
    }
}