using ECommerceApi.Data;
using ECommerceApi.DTOs;
using ECommerceApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ECommerceApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CartController : ControllerBase
    {
        readonly AppDbContext _context;

        public CartController(AppDbContext context)
        {
            _context = context;
        }

        private async Task<Cart> GetOrCreateCartAsync(string userId)
        {
            var cart = await _context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
            {
                cart = new Cart { UserId = userId };
                _context.Carts.Add(cart);
                await _context.SaveChangesAsync();
            }

            return cart;
        }

        [HttpGet]
        public async Task<IActionResult> GetCart()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var cart = await GetOrCreateCartAsync(userId);
            return Ok(new
            {
                cart.Id,
                cart.UserId,
                Items = cart.Items.Select(i => new { i.Id, i.ProductId, i.Quantity })
            });
        }

        [HttpPost]
        public async Task<IActionResult> AddItem([FromBody] AddCartItemDto dto)
        {
            try
            {
                if(!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

                var product = await _context.Products.FindAsync(dto.ProductId);
                
                if(product == null)
                {
                    return NotFound();
                }

                var cart = await GetOrCreateCartAsync(userId);

                var existingItem = cart.Items.FirstOrDefault(ci => ci.ProductId == dto.ProductId);

                if(existingItem != null)
                {
                    existingItem.Quantity += dto.Quantity;
                }
                else
                {
                    var newItem = new CartItem
                    {
                        CartId = cart.Id,
                        ProductId = dto.ProductId,
                        Quantity = dto.Quantity,
                    };
                    _context.CartItems.Add(newItem);
                }
                
                await _context.SaveChangesAsync();
                return Ok(new
                {
                    cart.Id,
                    cart.UserId,
                    Items = cart.Items.Select(i => new { i.Id, i.ProductId, i.Quantity })
                });


            }
            catch (Exception ex) 
            {
                return StatusCode(500, new { error = "Something went wrong" });
            }
        }
        [HttpDelete("{productId}")]

        public async Task<IActionResult> RemoveItem(int productId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var cart = await GetOrCreateCartAsync(userId);

            var item = cart.Items.FirstOrDefault(ci => ci.ProductId == productId);

            if (item == null)
            {
                return NotFound();
            }

            _context.CartItems.Remove(item);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Item removed from cart." });


        }

    }
}
