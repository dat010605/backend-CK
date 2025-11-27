using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniOrderAPI.Data;
using MiniOrderAPI.DTOs;
using MiniOrderAPI.Models;
using System.Security.Claims; 

namespace MiniOrderAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] 
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
            // 1. Lấy thông tin User từ Token
            var username = User.Identity?.Name;
            var currentUser = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);

            if (currentUser == null) return Unauthorized("Không xác định được người dùng.");

            // 2. Tạo Order Master (Sử dụng UserId thay vì CustomerId)
            var order = new Order
            {
                UserId = currentUser.Id, // <--- ĐÃ SỬA: Gán ID của User đang đăng nhập
                OrderDate = DateTime.Now,
                Status = "New",
                OrderDetails = new List<OrderDetail>()
            };

            decimal totalAmount = 0;

            // 3. Duyệt sản phẩm và trừ tồn kho
            foreach (var item in dto.Items)
            {
                var product = await _context.Products.FindAsync(item.ProductId);
                
                // Kiểm tra sản phẩm tồn tại
                if (product == null)
                    return NotFound(new { message = $"Không tìm thấy sản phẩm ID: {item.ProductId}" });

                // Kiểm tra số lượng tồn
                if (product.StockQuantity < item.Quantity)
                    return BadRequest(new { message = $"Sản phẩm '{product.Name}' không đủ hàng (còn {product.StockQuantity})" });

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

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            // Trả về kết quả
            return CreatedAtAction(nameof(GetById), new { id = order.Id }, order);
        }

        // GET: api/orders/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var order = await _context.Orders
                .Include(o => o.User)         // Include thông tin người đặt
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null) return NotFound(new { message = "Không tìm thấy đơn hàng" });

            // 4. PHÂN QUYỀN:
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            var username = User.Identity?.Name;
            
            var currentUser = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            
            // Nếu không phải Admin VÀ đơn hàng này không phải của người đó -> Chặn
            if (userRole != "Admin" && order.UserId != currentUser.Id) // <--- ĐÃ SỬA: So sánh UserId
            {
                return Forbid(); 
            }

            return Ok(order);
        }

        // GET: api/orders
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Order>>> GetAll()
        {
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            var username = User.Identity?.Name;
            var currentUser = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);

            IQueryable<Order> query = _context.Orders
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
                .OrderByDescending(o => o.OrderDate);

            // 5. LỌC DANH SÁCH:
            // Nếu là User thường, chỉ thấy đơn của chính mình
            if (userRole != "Admin")
            {
                query = query.Where(o => o.UserId == currentUser.Id); // <--- ĐÃ SỬA: Lọc theo UserId
            }

            return await query.ToListAsync();
        }

        // DELETE: api/orders/{id} – Chỉ Admin
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteOrder(int id)
        {
            var order = await _context.Orders
                .Include(o => o.OrderDetails)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null) return NotFound(new { message = "Không tìm thấy đơn hàng" });

            // Hoàn lại tồn kho khi xóa đơn
            foreach(var item in order.OrderDetails)
            {
                var product = await _context.Products.FindAsync(item.ProductId);
                if(product != null) 
                {
                    product.StockQuantity += item.Quantity;
                }
            }

            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Đã xóa đơn hàng thành công" });
        }
    }
}