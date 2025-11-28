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

        // POST: api/orders (Tạo đơn)
        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderDto dto)
        {
            var username = User.Identity?.Name;
            var currentUser = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (currentUser == null) return Unauthorized("Không xác định được người dùng.");

            var order = new Order
            {
                UserId = currentUser.Id,
                OrderDate = DateTime.Now,
                Status = "Chờ duyệt", // Trạng thái mặc định tiếng Việt cho thân thiện
                OrderDetails = new List<OrderDetail>()
            };

            decimal totalAmount = 0;

            foreach (var item in dto.Items)
            {
                var product = await _context.Products.FindAsync(item.ProductId);
                if (product == null) return NotFound(new { message = $"Sản phẩm ID {item.ProductId} không tồn tại" });
                if (product.StockQuantity < item.Quantity) return BadRequest(new { message = $"Sản phẩm {product.Name} không đủ hàng." });

                var detail = new OrderDetail
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    UnitPrice = product.Price
                };

                order.OrderDetails.Add(detail);
                totalAmount += detail.Quantity * detail.UnitPrice;
                product.StockQuantity -= item.Quantity; // Trừ kho
            }

            order.TotalAmount = totalAmount;
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = order.Id }, order);
        }

        // GET: api/orders/{id} (Chi tiết đơn)
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var order = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderDetails).ThenInclude(od => od.Product)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null) return NotFound(new { message = "Không tìm thấy đơn hàng" });

            // Check quyền: Admin hoặc Chủ đơn hàng mới được xem
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            var username = User.Identity?.Name;
            var currentUser = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);

            if (userRole != "Admin" && order.UserId != currentUser.Id) return Forbid();

            return Ok(order);
        }

        // GET: api/orders (Danh sách đơn)
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Order>>> GetAll()
        {
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            var username = User.Identity?.Name;
            var currentUser = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);

            IQueryable<Order> query = _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderDetails).ThenInclude(od => od.Product)
                .OrderByDescending(o => o.OrderDate);

            // Nếu không phải Admin thì chỉ xem đơn của mình
            if (userRole != "Admin")
            {
                query = query.Where(o => o.UserId == currentUser.Id);
            }

            return await query.ToListAsync();
        }

        // PUT: api/orders/{id}/status (Cập nhật trạng thái - Chỉ Admin)
        [HttpPut("{id}/status")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] string status)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null) return NotFound();
            
            order.Status = status;
            await _context.SaveChangesAsync();
            return Ok(new { message = "Cập nhật trạng thái thành công" });
        }

        // DELETE: api/orders/{id} (Xóa đơn - Chỉ Admin)
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteOrder(int id)
        {
            var order = await _context.Orders.Include(o => o.OrderDetails).FirstOrDefaultAsync(o => o.Id == id);
            if (order == null) return NotFound();

            // Hoàn lại kho khi xóa
            foreach(var item in order.OrderDetails)
            {
                var p = await _context.Products.FindAsync(item.ProductId);
                if(p != null) p.StockQuantity += item.Quantity;
            }

            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Đã xóa đơn hàng" });
        }
    }
}