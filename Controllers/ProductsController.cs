using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniOrderAPI.Data;
using MiniOrderAPI.DTOs;
using MiniOrderAPI.Models;
using Microsoft.AspNetCore.Authorization; // Cần thiết cho [Authorize]

namespace MiniOrderAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ProductsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/products
        // Ai cũng có thể xem danh sách sản phẩm (hoặc yêu cầu đăng nhập tùy bạn)
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _context.Products.ToListAsync());
        }

        // GET: api/products/5
        // Thêm hàm này để CreatedAtAction hoạt động đúng
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();
            return Ok(product);
        }

        // POST: api/products
        // Chỉ Admin mới được thêm
        [HttpPost]
        [Authorize(Roles = "Admin")] 
        public async Task<IActionResult> Create([FromBody] CreateProductDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var product = new Product
            {
                Name = dto.Name,
                Price = dto.Price,
                StockQuantity = dto.StockQuantity,
                Description = dto.Description
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            // Trả về đường dẫn tới sản phẩm vừa tạo
            return CreatedAtAction(nameof(GetById), new { id = product.Id }, product);
        }

        // PUT: api/products/5
        // Chỉ Admin mới được sửa
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, [FromBody] CreateProductDto dto)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) 
                return NotFound(new { message = "Không tìm thấy sản phẩm" });

            // Cập nhật thông tin
            product.Name = dto.Name;
            product.Price = dto.Price;
            product.StockQuantity = dto.StockQuantity;
            product.Description = dto.Description;

            await _context.SaveChangesAsync();
            return Ok(new { message = "Cập nhật thành công", product });
        }

        // DELETE: api/products/5
        // Chỉ Admin mới được xóa
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) 
                return NotFound(new { message = "Không tìm thấy sản phẩm" });

            // Kiểm tra xem sản phẩm có đang nằm trong đơn hàng nào không (tùy chọn)
            // Nếu muốn xóa triệt để thì cứ xóa, nhưng nên cẩn thận ràng buộc khóa ngoại
            _context.Products.Remove(product);
            
            try 
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return BadRequest(new { message = "Không thể xóa sản phẩm này vì đã có đơn hàng liên quan." });
            }

            return Ok(new { message = "Đã xóa sản phẩm" });
        }
    }
}