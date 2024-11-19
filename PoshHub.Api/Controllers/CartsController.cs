using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PoshHub.Data.Models;
using PoshHub.Data.Context;

namespace PoshHub.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CartController : ControllerBase
{
    private readonly PoshHubContext _context;

    public CartController(PoshHubContext context)
    {
        _context = context;
    }

    // Додавання товару в кошик
    [HttpPost("add")]
    public async Task<IActionResult> AddToCart(int userId, int productId, int quantity)
    {
        var cart = await _context.Carts
            .Include(c => c.CartItems)
            .FirstOrDefaultAsync(c => c.UserId == userId);

        if (cart == null)
        {
            // Створюємо новий кошик для користувача, якщо його немає
            cart = new Cart
            {
                UserId = userId,
                LastUpdated = DateTime.UtcNow,
                CartItems = new List<CartItem>()
            };
            _context.Carts.Add(cart);
            await _context.SaveChangesAsync();
        }

        var cartItem = cart.CartItems.FirstOrDefault(ci => ci.ProductId == productId);

        if (cartItem != null)
        {
            // Оновлюємо кількість товару в кошику
            cartItem.Quantity += quantity;
        }
        else
        {
            // Додаємо новий товар у кошик
            cart.CartItems.Add(new CartItem
            {
                ProductId = productId,
                Quantity = quantity
            });
        }

        cart.LastUpdated = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return Ok(cart);
    }

    // Перегляд кошика
    [HttpGet("view/{userId}")]
    public async Task<IActionResult> ViewCart(int userId)
    {
        var cart = await _context.Carts
            .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
            .FirstOrDefaultAsync(c => c.UserId == userId);

        if (cart == null)
        {
            return NotFound("Кошик не знайдено.");
        }

        return Ok(cart);
    }

    // Видалення товару з кошика
    [HttpDelete("remove")]
    public async Task<IActionResult> RemoveFromCart(int userId, int productId)
    {
        var cart = await _context.Carts
            .Include(c => c.CartItems)
            .FirstOrDefaultAsync(c => c.UserId == userId);

        if (cart == null)
        {
            return NotFound("Кошик не знайдено.");
        }

        var cartItem = cart.CartItems.FirstOrDefault(ci => ci.ProductId == productId);

        if (cartItem == null)
        {
            return NotFound("Товар не знайдений в кошику.");
        }

        cart.CartItems.Remove(cartItem);
        cart.LastUpdated = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return Ok(cart);
    }
}
