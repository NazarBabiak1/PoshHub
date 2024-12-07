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
    public class ReviewsController : ControllerBase
    {
        private readonly PoshHubContext _context;

        public ReviewsController(PoshHubContext context)
        {
            _context = context;
        }

        // Get all reviews for a product
        [HttpGet("product/{productId}")]
        public async Task<IActionResult> GetReviewsForProduct(int productId)
        {
            var reviews = await _context.Reviews
                .Where(r => r.ProductId == productId)
                .Select(r => new
                {
                    r.Id,
                    r.UserId,
                    UserName = r.User.FirstName,
                    r.Rating,
                    r.Comment,
                    r.CreatedAt
                })
                .ToListAsync();

            return Ok(reviews);
        }

        // Add a review
        [HttpPost("product/{productId}")]
        [Authorize]
        public async Task<IActionResult> AddReview(int productId, [FromBody] Review newReview)
        {
            var userId = int.Parse(User.Identity.Name);

            var productExists = await _context.Products.AnyAsync(p => p.Id == productId);
            if (!productExists)
                return NotFound("Product not found.");

            var review = new Review
            {
                ProductId = productId,
                UserId = userId,
                Rating = newReview.Rating,
                Comment = newReview.Comment,
                CreatedAt = DateTime.Now
            };

            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetReviewsForProduct), new { productId }, review);
        }

        // Edit a review
        [HttpPut("{reviewId}")]
        [Authorize]
        public async Task<IActionResult> EditReview(int reviewId, [FromBody] Review updatedReview)
        {
            var userId = int.Parse(User.Identity.Name);

            var review = await _context.Reviews.FindAsync(reviewId);
            if (review == null || review.UserId != userId)
                return NotFound("Review not found or unauthorized.");

            review.Rating = updatedReview.Rating;
            review.Comment = updatedReview.Comment;
            review.CreatedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            return Ok("Review updated successfully.");
        }

        // Delete a review
        [HttpDelete("{reviewId}")]
        [Authorize]
        public async Task<IActionResult> DeleteReview(int reviewId)
        {
            var userId = int.Parse(User.Identity.Name);

            var review = await _context.Reviews.FindAsync(reviewId);
            if (review == null || review.UserId != userId)
                return NotFound("Review not found or unauthorized.");

            _context.Reviews.Remove(review);
            await _context.SaveChangesAsync();

            return Ok("Review deleted successfully.");
        }
    }
}
