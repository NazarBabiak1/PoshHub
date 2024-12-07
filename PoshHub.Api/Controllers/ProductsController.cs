using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PoshHub.Data;
using PoshHub.Data.Context;
using PoshHub.Data.Models;
using System.Linq;
using System.Threading.Tasks;

namespace PoshHub.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly PoshHubContext _context;

        public ProductsController(PoshHubContext context)
        {
            _context = context;
        }

        // Get all products (catalog)
        [HttpGet]
        public async Task<IActionResult> GetProducts(string? search, string? category, decimal? minPrice, decimal? maxPrice)
        {
            var query = _context.Products.AsQueryable();

            if (!string.IsNullOrEmpty(search))
                query = query.Where(p => p.Name.Contains(search) || p.Description.Contains(search));

            if (!string.IsNullOrEmpty(category))
                query = query.Where(p => p.Category == category);

            if (minPrice.HasValue)
                query = query.Where(p => p.Price >= minPrice);

            if (maxPrice.HasValue)
                query = query.Where(p => p.Price <= maxPrice);

            var products = await query.Select(p => new
            {
                p.Id,
                p.Name,
                p.Price,
                p.StockQuantity,
                p.Category,
                p.ImageUrl
            }).ToListAsync();

            return Ok(products);
        }

        // Get product details
        [HttpGet("{id}")]
        public async Task<IActionResult> GetProductDetails(int id)
        {
            var product = await _context.Products
                .Include(p => p.Reviews)
                .ThenInclude(r => r.User)
                .Where(p => p.Id == id)
                .Select(p => new
                {
                    p.Id,
                    p.Name,
                    p.Description,
                    p.Price,
                    p.StockQuantity,
                    p.Category,
                    p.ImageUrl,
                    Reviews = p.Reviews.Select(r => new
                    {
                        r.Id,
                        r.UserId,
                        r.User.FirstName,
                        r.Rating,
                        r.Comment,
                        r.CreatedAt
                    })
                })
                .FirstOrDefaultAsync();

            if (product == null)
                return NotFound("Product not found.");

            return Ok(product);
        }

        // Add a review for a product
        [HttpPost("{id}/reviews")]
        [Authorize]
        public async Task<IActionResult> AddReview(int id, [FromBody] Review newReview)
        {
            var userId = int.Parse(User.Identity.Name);

            var product = await _context.Products.FindAsync(id);
            if (product == null)
                return NotFound("Product not found.");

            var review = new Review
            {
                ProductId = id,
                UserId = userId,
                Rating = newReview.Rating,
                Comment = newReview.Comment,
                CreatedAt = DateTime.Now
            };

            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetProductDetails), new { id = id }, review);
        }

    }
}
