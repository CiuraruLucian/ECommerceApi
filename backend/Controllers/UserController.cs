using ECommerceApi.DTOs;
using ECommerceApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace ECommerceApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : Controller
    {
        private readonly UserManager<User> _userManager;

        public UserController(UserManager<User> userManager)
        {
            _userManager = userManager;
        }


        [Authorize(Roles = "Admin")]

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var users = await _userManager.Users.Select(u => new { u.Id, u.UserName, u.Email }).ToListAsync();

            return Ok(users);
        }

        [Authorize(Roles = "Admin")]

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id) 
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            return Ok(new {user.Id, user.UserName, user.Email});

        }

        [Authorize(Roles = "Admin")]

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(string id, [FromBody] UpdateUserDto dto )
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            if (!string.IsNullOrWhiteSpace(dto.Email))
            {
                user.Email = dto.Email;
            }
            if (!string.IsNullOrWhiteSpace(dto.UserName))
            {
                user.UserName = dto.UserName;
            }
            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                return BadRequest( result.Errors);
            }
            return Ok(new {user.Id, user.UserName, user.Email});


        }

        [Authorize(Roles = "Admin")]

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                return BadRequest( result.Errors);
            }
            return Ok(new { message = "User deleted successfully." });

        }
    }
}
