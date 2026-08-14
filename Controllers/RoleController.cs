using ECommerceApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class RoleController : Controller
    {

        private readonly UserManager<User> _userManger;

        public RoleController(UserManager<User> userManager)
        {
            _userManger = userManager;
        }
        [HttpPost("assign")]
        public async Task<IActionResult> AssignRole([FromBody] AssignRoleDto dto)
        {
            try
            {
                var user = await _userManger.FindByIdAsync(dto.UserId);


                if (user == null)
                {
                    return NotFound(new { error = "Invalid username" });
                }

                var roleAssign = await _userManger.AddToRoleAsync(user, dto.RoleName);

                if (!roleAssign.Succeeded)
                {
                    return BadRequest(new { error = "Invalid role" });
                }

                return Ok(new { message = "Role added succesfully." } );
               
            }
            catch (Exception)
            {
                return StatusCode(500, new { error = "Something went wrong" });
            }
        }
    }
}
