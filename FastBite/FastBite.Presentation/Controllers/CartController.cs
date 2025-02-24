using FastBite.Core.Interfaces;
using FastBite.Shared.DTOS;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/v1/[controller]")]
public class CartController : ControllerBase
{
    private readonly IProductService _productService;

    public CartController(IProductService productService)
    {
        _productService = productService;
    }
    
    [HttpGet("userCart")]
    public async Task<IActionResult> GetUserCartAsync([FromBody] string userId)
    {
        try
        {
            var products = await _productService.GetUserCartAsync(userId);
            
            if (products == null || !products.Any())
            {
                return NotFound(new { message = "Корзина пуста" });
            }

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
        return Ok("Product added to cart");
    }

    [HttpDelete("removeFromCart")]
    public async Task<IActionResult> RemoveProductFromCart([FromBody] CartRequestDTO request)
    {
        await _productService.RemoveProductFromCartAsync(request.UserId, request.ProductId);
        return Ok("Product removed from cart");
    }

    [HttpDelete("clearCart")]
    public async Task<IActionResult> ClearCart([FromBody] string userId)
    {
        await _productService.ClearCartAsync(userId);
        return Ok("Cart cleared");
    }
}