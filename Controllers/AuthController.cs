using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ECommerceApi.Models;
using BCrypt.Net;
using ECommerceApi.Data;
using Microsoft.EntityFrameworkCore;
using ECommerceApi.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;

namespace ECommerceApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<User> _userManager;

        private readonly SignInManager<User> _signInManager;
        
        
        private readonly JwtService _jwtService;

        

        public AuthController( JwtService jwtService, UserManager<User> userManager, SignInManager<User> signInManager)
        {
            
            _jwtService = jwtService;
            _userManager = userManager;
            _signInManager = signInManager;
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

                var user = new User { Email = normalizedEmail, UserName = dto.Username, Role = "Customer" };

                var response = await _userManager.CreateAsync(user,dto.Password);

                if (!response.Succeeded)
                {
                    return BadRequest(response.Errors);
                }

                return Created("", new {user.Id, user.Email, user.UserName } );
                
            }
            catch(Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPost("login")]

        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            try
            {
                var user = await _userManager.FindByNameAsync(dto.Username);

                if(user == null)
                {
                    return Unauthorized(new { error = "Invalid username or password" });
                }

                var passwordResult = await _signInManager.CheckPasswordSignInAsync(user, dto.Password, lockoutOnFailure: false);
                
                if(!passwordResult.Succeeded)
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

        [Authorize]

        [HttpGet("testLogin")]

        public IActionResult testLogin()
        {
            
            return Ok(new { message = "You are authenticated!", user = User.Identity?.Name });
        }
    }
}
