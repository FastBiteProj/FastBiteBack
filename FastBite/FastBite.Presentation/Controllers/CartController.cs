using FastBite.Core.Interfaces;
using FastBite.Infastructure.Hubs;
using FastBite.Shared.DTOS;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

[ApiController]
[Route("api/v1/[controller]")]
public class CartController : ControllerBase
{
    private readonly IProductService _productService;
    private readonly IHubContext<CartHub> _hubContext;
    public CartController(IProductService productService, IHubContext<CartHub> hubContext)
    {
        _productService = productService;
        _hubContext = hubContext;
    }
    
    [HttpGet("userCart")]
    public async Task<IActionResult> GetUserCartAsync([FromQuery] string userId)
    {
        try
        {
            var products = await _productService.GetUserCartAsync(userId);

            return Ok(products);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Внутренняя ошибка сервера", ex });
        }
    }
    
    [HttpPost("addProductsToCart")]
    public async Task<IActionResult> AddProductToCart([FromBody] CartRequestDTO request)
    {
        await _productService.AddProductToCartAsync(request.UserId, request.ProductId);
        await _hubContext.Clients.All.SendAsync("CartUpdated", request.UserId);
        return Ok("Product added to cart");
    }

    [HttpDelete("removeFromCart")]
    public async Task<IActionResult> RemoveProductFromCart([FromBody] CartRequestDTO request)
    {
        await _productService.RemoveProductFromCartAsync(request.UserId, request.ProductId);
        await _hubContext.Clients.All.SendAsync("CartUpdated", request.UserId);
        return Ok("Product removed from cart");
    }

    [HttpDelete("clearCart")]
    public async Task<IActionResult> ClearCart([FromBody] string userId)
    {
        await _productService.ClearCartAsync(userId);
        await _hubContext.Clients.All.SendAsync("CartUpdated", userId);
        return Ok("Cart cleared");
    }
}