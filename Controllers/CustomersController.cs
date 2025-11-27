using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniOrderAPI.Data;
using MiniOrderAPI.DTOs;
using MiniOrderAPI.Models;

namespace MiniOrderAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")] // <--- QUAN TRỌNG: Chỉ Admin mới được quản lý Customer
    public class CustomersController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CustomersController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/customers
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Customer>>> GetAll()
        {
            return await _context.Customers.ToListAsync();
        }

        // GET: api/customers/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Customer>> GetById(int id)
        {
            var customer = await _context.Customers.FindAsync(id);

            if (customer == null)
            {
                return NotFound(new { message = $"Không tìm thấy khách hàng ID = {id}" });
            }

            return Ok(customer);
        }

        // POST: api/customers
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateCustomerDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var customer = new Customer
            {
                Name = dto.Name,
                Email = dto.Email,
                Phone = dto.Phone,
                Address = dto.Address
            };

            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = customer.Id }, customer);
        }

        // PUT: api/customers/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] CreateCustomerDto dto) // Dùng tạm CreateCustomerDto nếu chưa có UpdateDto
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var customer = await _context.Customers.FindAsync(id);
            if (customer == null)
            {
                return NotFound(new { message = $"Không tìm thấy khách hàng ID = {id}" });
            }

            customer.Name = dto.Name;
            customer.Email = dto.Email;
            customer.Phone = dto.Phone;
            customer.Address = dto.Address;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                return Conflict(new { message = "Dữ liệu đã bị thay đổi bởi người khác." });
            }

            return Ok(new { message = "Cập nhật thành công", customer });
        }

        // DELETE: api/customers/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var customer = await _context.Customers.FindAsync(id);
            if (customer == null)
            {
                return NotFound(new { message = $"Không tìm thấy khách hàng ID = {id}" });
            }

            _context.Customers.Remove(customer);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Đã xóa khách hàng thành công" });
        }
    }
}