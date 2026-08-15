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
               

                Product product = new Product { Name = dto.Name, NormalizedName = NormalizedName, Description = dto.Description, Price = dto.Price, Stock = dto.Stock };
            
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
        public async Task<IActionResult>  Put(int id, [FromBody] ProductDto dto)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            if (!string.IsNullOrWhiteSpace(dto.Name))
            {
                product.Name = dto.Name;
                product.NormalizedName = dto.Name.Trim().ToLower();
            }
            if (!string.IsNullOrWhiteSpace(dto.Description))
            {
                product.Description = dto.Description;
            }
            if (!decimal.IsNegative(dto.Price))
            {
                product.Price = dto.Price;
            }
            if (!int.IsNegative(dto.Stock))
            {
                product.Stock = dto.Stock;
            }
            await _context.SaveChangesAsync();
            
            return Ok(new { product.Id, product.Name, product.Description, product.Price, product.Stock });


        }

        [Authorize(Roles = "Admin")]

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            _context.Products.Remove(product);

            await _context.SaveChangesAsync();

            return Ok(new { message = "Product deleted successfully." });
        }
    }
}
