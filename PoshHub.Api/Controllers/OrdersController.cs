using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using PoshHub.Data;
using PoshHub.Data.Context;
using PoshHub.Data.Models;
using System.Linq;
using System.Threading.Tasks;

namespace PoshHub.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class OrdersController : ControllerBase
    {
        private readonly PoshHubContext _context;

        public OrdersController(PoshHubContext context)
        {
            _context = context;
        }

        // Отримати всі замовлення користувача
        [HttpGet]
        public async Task<IActionResult> GetOrders()
        {
            var userId = int.Parse(User.Identity.Name); // Ідентифікація користувача
            var orders = _context.Orders
                .Where(o => o.UserId == userId)
                .Select(o => new
                {
                    o.Id,
                    o.OrderDate,
                    o.TotalAmount,
                    o.Status
                });

            return Ok(await orders.ToListAsync());
        }

        // Деталі конкретного замовлення
        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrderDetails(int id)
        {
            var userId = int.Parse(User.Identity.Name);
            var order = await _context.Orders
                .Where(o => o.Id == id && o.UserId == userId)
                .Select(o => new
                {
                    o.Id,
                    o.OrderDate,
                    o.TotalAmount,
                    o.Status,
                    o.ShippingAddress,
                    Items = o.OrderItems.Select(oi => new
                    {
                        oi.Product.Name,
                        oi.Quantity,
                        oi.Product.Price
                    })
                })
                .FirstOrDefaultAsync();

            if (order == null)
                return NotFound("Замовлення не знайдено.");

            return Ok(order);
        }

        // Створити нове замовлення
        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] Order newOrder)
        {
            var userId = int.Parse(User.Identity.Name);

            // Перевірити, чи кошик не пустий
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null || !cart.CartItems.Any())
                return BadRequest("Кошик пустий.");

            // Створити замовлення
            var order = new Order
            {
                UserId = userId,
                OrderDate = DateTime.Now,
                Status = "В очікуванні",
                ShippingAddress = newOrder.ShippingAddress,
                TotalAmount = cart.CartItems.Sum(ci => ci.Product.Price * ci.Quantity),
                OrderItems = cart.CartItems.Select(ci => new OrderItem
                {
                    ProductId = ci.ProductId,
                    Quantity = ci.Quantity
                }).ToList()
            };

            _context.Orders.Add(order);

            // Очистити кошик після створення замовлення
            //_context.Carts.RemoveRange(cart.CartItems);
            cart.LastUpdated = DateTime.Now;
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetOrderDetails), new { id = order.Id }, order);
        }
    }
}
