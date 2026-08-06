using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ECommerceApi.Models;
using BCrypt.Net;
using ECommerceApi.Data;
using Microsoft.EntityFrameworkCore;
using ECommerceApi.Services;

namespace ECommerceApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly JwtService _jwtService;

        private readonly AppDbContext _context;

        public AuthController(AppDbContext context, JwtService jwtService)
        {
            _context = context;
            _jwtService = jwtService;
        }

        [HttpPost("register")]

        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            try
            {
                if (!ModelState.IsValid) 
                { 
                    return BadRequest(ModelState);
                }
                
                string normalizedEmail = dto.Email.Trim().ToLower();
                
                
                if(await _context.Users.AnyAsync( u => u.Email == normalizedEmail))     
                {
                    return StatusCode(400, new { error = "This email already exists"});
                }
                
                if(await _context.Users.AnyAsync( u => u.Username == dto.Username))
                {
                    return StatusCode(400, new { error = "This username already exists" });
                }
                
                string hash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

                User user = new User { Email = normalizedEmail, PasswordHash = hash, Username = dto.Username, Role = "Customer" };

                _context.Users.Add(user);

                var response = await _context.SaveChangesAsync();

                return Created("", new {user.Id, user.Email, user.Username} );
                
            }
            catch(Exception)
            {
                return StatusCode(500, new { error = "Something went wrong" });
            }
        }

        [HttpPost("login")]

        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == dto.Username);

                if(user == null)
                {
                    return Unauthorized(new { error = "Invalid username or password" });
                }

                bool passwordValid = BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash);
                if (!passwordValid)
                {
                    return Unauthorized(new { error = "Invalid username or password" });
                }

                var token = _jwtService.GenerateToken(user);
                return Ok(new
                {
                    token = token
                });
            }
            catch(Exception)
            {
                return StatusCode(500, new { error = "Something went wrong" });
            }
        }
    }
}
