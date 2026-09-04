using ECommerceApi.Data;
using ECommerceApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Stripe;
using System.Security.Claims;

namespace ECommerceApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class OrderController : ControllerBase
    {
        private readonly AppDbContext _context;

        public OrderController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]

        public async Task<IActionResult> GetMyOrders()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            var orders = await _context.Orders
                .Include(o => o.Items)
                .Where(o => o.UserId == userId)
                .Select(o => new
                {
                    o.Id,
                    o.Total,
                    o.Status,
                    Items = o.Items.Select(i => new { i.ProductName, i.UnitPrice, i.Quantity })
                })
                .ToListAsync();

            if (!orders.Any())
            {
                return NotFound();
            }

            return Ok(orders);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrderById(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            var order = await _context.Orders
                .Include(o => o.Items)
                .Where(o => o.Id == id && o.UserId == userId)
                .Select(o => new
                {
                    o.Id,
                    o.Total,
                    o.Status,
                    Items = o.Items.Select(i => new { i.ProductName, i.UnitPrice, i.Quantity })
                })
                .ToListAsync();
            if (order == null)
            {
                return NotFound();
            }
            return Ok(order);
        }




        [HttpPost("checkout")]

        public async  Task<IActionResult> Checkout()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

                var cart = await _context.Carts
                    .Include(c => c.Items)
                    .FirstOrDefaultAsync(c => c.UserId == userId);

                if (cart == null || !cart.Items.Any())
                {
                    return BadRequest(new { error = "Cart is empty." });
                }

                var orderItems = new List<OrderItem>();

                decimal total = 0;

                foreach (var cartItem in cart.Items)
                {
                    var product = await _context.Products.FindAsync(cartItem.ProductId);

                    if (product == null)
                    {
                        continue;
                    }

                    var orderItem = new OrderItem
                    {
                        ProductId = cartItem.ProductId,
                        ProductName = product.Name,
                        UnitPrice = product.Price,
                        Quantity = cartItem.Quantity

                    };
                    if (product.Stock < cartItem.Quantity)
                    {
                        return BadRequest(new { error = $"Insufficient stock for {product.Name}" });
                    }

                    product.Stock -= cartItem.Quantity;

                    orderItems.Add(orderItem);
                    total += product.Price * cartItem.Quantity;
                }

                var options = new PaymentIntentCreateOptions
                {
                    Amount = (long)(total * 100),
                    Currency = "gbp",
                    PaymentMethodTypes = new List<string> { "card" }
                };

                var service = new PaymentIntentService();

                PaymentIntent intent = await service.CreateAsync(options);



                var order = new Order
                {
                    UserId = userId,
                    Total = total,
                    Items = orderItems,
                    PaymentIntentId = intent.Id,
                    Status = "Pending"
                };

                _context.Orders.Add(order);
                _context.CartItems.RemoveRange(cart.Items);
                await _context.SaveChangesAsync();

                return Ok(new { order.Id, order.Total, clientSecret = intent.ClientSecret, order.Status, Items = orderItems.Select(i => new { i.ProductId, i.ProductName, i.UnitPrice, i.Quantity }) });
            }
            catch (Exception)
            {
                return StatusCode(500, new {error = "Something went wrong."});
            }
        }

        [HttpPost("{id}/confirm-payment")]

        public async Task<IActionResult> ConfirmPayment(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            var order = await _context.Orders.FirstOrDefaultAsync(o =>  o.Id == id && o.UserId == userId);

            if (order == null)
            {
                return NotFound();
            }

            var service = new PaymentIntentService();

            var intent = await service.GetAsync(order.PaymentIntentId);

            if(intent.Status == "succeeded")
            {
                order.Status = "Paid";
                await _context.SaveChangesAsync();
            }

            return Ok( new { order.Id, order.Status, stripeStatus = intent.Status  });

        }

    }
}
