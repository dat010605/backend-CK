using Microsoft.AspNetCore.Mvc;           // Dùng cho [Route], [ApiController], ControllerBase
using Microsoft.EntityFrameworkCore;      // Dùng cho ToListAsync
using MiniOrderAPI.Data;                  // Dùng cho AppDbContext
using MiniOrderAPI.DTOs;                  // Dùng cho CreateProductDto
using MiniOrderAPI.Models;                // Dùng cho Product

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
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _context.Products.ToListAsync());
        }

        // POST: api/products
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateProductDto dto)
        {
            var product = new Product
            {
                Name = dto.Name,
                Price = dto.Price,
                StockQuantity = dto.StockQuantity,
                Description = dto.Description
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetAll), new { id = product.Id }, product);
        }
    }
}