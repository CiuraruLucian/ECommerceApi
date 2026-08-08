using ECommerceApi.Data;
using ECommerceApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace ECommerceApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        readonly AppDbContext _context;

        public ProductsController(AppDbContext context)
        {
            _context = context;
        }




        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var products = await _context.Products.ToListAsync();
            return Ok(products);
        }

        
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if(product == null)
            {
                return NotFound();
            }
            return Ok(product);
        }

        [Authorize(Roles = "Admin")]

        [HttpPost]
        public async Task<IActionResult> AddProduct([FromBody] ProductDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                string Name = dto.Name.Trim();

                string NormalizedName = dto.Name.Trim().ToLower();

                if (await _context.Products.AnyAsync (p => p.Name == NormalizedName))
                {
                    return StatusCode(400, new { error = "This product already exists" });
                }
               

                Product product = new Product { Name = dto.Name, Description = dto.Description, Price = dto.Price, Stock = dto.Stock };
            
                _context.Products.Add(product); 

                var response = await _context.SaveChangesAsync();

                return Created("", new { product.Id, product.Name, product.Description, product.Price, product.Stock });
            
            }
            catch (Exception) 
            {
                return StatusCode(500, new { error = "Something went wrong." });
            }



        }

        [Authorize(Roles = "Admin")]

        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        [Authorize(Roles = "Admin")]

        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
