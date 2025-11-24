using Microsoft.AspNetCore.Mvc;           // Để dùng [HttpPost], ControllerBase, IActionResult
using Microsoft.EntityFrameworkCore;      // Để dùng .Include(), .ToListAsync()
using MiniOrderAPI.Data;                  // Để dùng AppDbContext
using MiniOrderAPI.DTOs;                  // Để dùng CreateOrderDto
using MiniOrderAPI.Models;                // Để dùng Order, OrderDetail, Product

namespace MiniOrderAPI.Controllers // Tên namespace project của bạn
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly AppDbContext _context;

        public OrdersController(AppDbContext context)
        {
            _context = context;
        }

        // POST: api/orders
        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderDto dto)
        {
            // 1. Tạo Order Master
            var order = new Order
            {
                CustomerId = dto.CustomerId,
                OrderDate = DateTime.Now,
                Status = "New",
                OrderDetails = new List<OrderDetail>()
            };

            decimal totalAmount = 0;

            // 2. Duyệt qua từng sản phẩm khách chọn
            foreach (var item in dto.Items)
            {
                var product = await _context.Products.FindAsync(item.ProductId);
                
                // Kiểm tra sản phẩm có tồn tại không
                if (product == null) 
                    return NotFound($"Không tìm thấy sản phẩm ID: {item.ProductId}");

                // Kiểm tra tồn kho (Optional)
                if (product.StockQuantity < item.Quantity)
                    return BadRequest($"Sản phẩm {product.Name} không đủ hàng.");

                // Tạo chi tiết đơn hàng
                var detail = new OrderDetail
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    UnitPrice = product.Price 
                };

                order.OrderDetails.Add(detail);
                totalAmount += detail.Quantity * detail.UnitPrice;
                
                // Trừ tồn kho
                product.StockQuantity -= item.Quantity;
            }

            order.TotalAmount = totalAmount;

            // 3. Lưu tất cả vào DB
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = order.Id }, order);
        }

        // GET: api/orders/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var order = await _context.Orders
                .Include(o => o.OrderDetails)       // Load kèm chi tiết
                .ThenInclude(od => od.Product)      // Load kèm tên sản phẩm trong chi tiết
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null) return NotFound();

            return Ok(order);
        }
    }
}