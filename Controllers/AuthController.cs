using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ECommerceApi.Models;
using BCrypt.Net;
using ECommerceApi.Data;

namespace ECommerceApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {

        private readonly AppDbContext _context;

        public AuthController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost("register")]

        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            try
            {
                if(_context.Users.Any( u => u.Email == dto.Email))
                {
                    return StatusCode(400, new { error = "This email already exists"});
                }else
                {
                    string password = dto.Password;

                    string hash = BCrypt.Net.BCrypt.HashPassword(password);

                    User user = new User { Email = dto.Email, PasswordHash = hash };

                    _context.Users.Add(user);

                    var response = await _context.SaveChangesAsync();

                    return Ok(new { user.Id, user.Email} );
                }
            }
            catch(Exception)
            {
                return StatusCode(500, new { error = "Something went wrong" });
            }
        }
    }
}
