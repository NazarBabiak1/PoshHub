using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PoshHub.Data.Context;
using PoshHub.Data.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace PoshHub.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly PoshHubContext _context;

        public UsersController(PoshHubContext context)
        {
            _context = context;
        }

        // User Registration
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] User newUser)
        {
            if (await _context.Users.AnyAsync(u => u.Email == newUser.Email))
                return BadRequest("Email already in use.");

            newUser.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newUser.PasswordHash);
            newUser.CreatedAt = DateTime.Now;

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            return Ok("Registration successful.");
        }

        // User Login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] User loginUser)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == loginUser.Email);
            if (user == null || !BCrypt.Net.BCrypt.Verify(loginUser.PasswordHash, user.PasswordHash))
                return Unauthorized("Invalid email or password.");

            return Ok("Login successful.");
        }

        // View User Profile
        [HttpGet("profile")]
        [Authorize]
        public async Task<IActionResult> GetProfile()
        {
            var userId = int.Parse(User.Identity.Name);

            var user = await _context.Users
                .Include(u => u.Cart)
                .Include(u => u.Orders)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                return NotFound("User not found.");

            return Ok(new
            {
                user.Id,
                user.FirstName,
                user.LastName,
                user.Email,
                user.PhoneNumber,
                user.Address,
                user.CreatedAt,
                CartId = user.Cart?.Id,
                Orders = user.Orders.Select(o => new
                {
                    o.Id,
                    o.OrderDate,
                    o.TotalAmount,
                    o.Status
                })
            });
        }

        // Edit User Profile
        [HttpPut("profile")]
        [Authorize]
        public async Task<IActionResult> EditProfile([FromBody] User updatedUser)
        {
            var userId = int.Parse(User.Identity.Name);

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return NotFound("User not found.");

            user.FirstName = updatedUser.FirstName;
            user.LastName = updatedUser.LastName;
            user.PhoneNumber = updatedUser.PhoneNumber;
            user.Address = updatedUser.Address;

            await _context.SaveChangesAsync();

            return Ok("Profile updated successfully.");
        }

        // Change Password
        [HttpPut("change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromBody] string newPassword)
        {
            var userId = int.Parse(User.Identity.Name);

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return NotFound("User not found.");

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);

            await _context.SaveChangesAsync();

            return Ok("Password changed successfully.");
        }
    }
}