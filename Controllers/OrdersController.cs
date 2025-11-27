using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniOrderAPI.Data;
using MiniOrderAPI.DTOs;
using MiniOrderAPI.Models;
using System.Security.Claims; // Cần để lấy thông tin từ Token

namespace MiniOrderAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // 1. Bắt buộc phải đăng nhập mới được vào Controller này
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
            // 2. LẤY ID NGƯỜI DÙNG TỪ TOKEN (An toàn tuyệt đối)
            var username = User.Identity?.Name;
            var currentUser = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);

            if (currentUser == null) return Unauthorized("Không xác định được người dùng.");

            // 3. Tạo Order Master
            var order = new Order
            {
                // Thay vì lấy dto.CustomerId, ta lấy chính ID của người đang đăng nhập
                CustomerId = currentUser.Id, 
                OrderDate = DateTime.Now,
                Status = "New",
                OrderDetails = new List<OrderDetail>()
            };

            decimal totalAmount = 0;

            // 4. Duyệt từng sản phẩm
            foreach (var item in dto.Items)
            {
                var product = await _context.Products.FindAsync(item.ProductId);
                if (product == null)
                    return NotFound($"Không tìm thấy sản phẩm ID: {item.ProductId}");

                if (product.StockQuantity < item.Quantity)
                    return BadRequest($"Sản phẩm {product.Name} không đủ hàng (còn {product.StockQuantity})");

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

            return CreatedAtAction(nameof(GetById), new { id = order.Id }, order);
        }

        // GET: api/orders/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var order = await _context.Orders
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null) return NotFound();

            // 5. PHÂN QUYỀN: Kiểm tra xem User có được xem đơn này không?
            // Admin được xem tất cả. User chỉ được xem đơn của mình.
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            var username = User.Identity?.Name;
            
            // Tìm User ID hiện tại
            var currentUser = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            
            if (userRole != "Admin" && order.CustomerId != currentUser.Id)
            {
                return Forbid(); // 403 Forbidden: Không có quyền xem đơn của người khác
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

            // 6. PHÂN QUYỀN DANH SÁCH:
            // Nếu không phải Admin, chỉ trả về đơn hàng CỦA CHÍNH HỌ
            if (userRole != "Admin")
            {
                query = query.Where(o => o.CustomerId == currentUser.Id);
            }

            return await query.ToListAsync();
        }

        // DELETE: api/orders/{id} – Chỉ Admin
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")] // Chỉ Admin mới xóa được
        public async Task<IActionResult> DeleteOrder(int id)
        {
            var order = await _context.Orders
                .Include(o => o.OrderDetails) // Load chi tiết để xóa (nếu cần hoàn lại kho)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null) return NotFound();

            // (Optional) Bonus: Hoàn lại tồn kho khi xóa đơn hàng
            foreach(var item in order.OrderDetails)
            {
                var product = await _context.Products.FindAsync(item.ProductId);
                if(product != null) product.StockQuantity += item.Quantity;
            }

            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Đã xóa đơn hàng thành công" });
        }
    }
}