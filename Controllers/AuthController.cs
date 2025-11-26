using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniOrderAPI.Data;    // <-- CHO AppDbContext
using MiniOrderAPI.Models;  // <-- nếu bạn dùng model User
using Microsoft.AspNetCore.Authorization;



namespace MiniOrderAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        // This controller intentionally left minimal. Login logic is implemented in `LoginController`.
        public AuthController()
        {
        }
    }
}
