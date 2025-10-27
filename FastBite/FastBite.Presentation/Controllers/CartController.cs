using FastBite.Core.Interfaces;
using FastBite.Infastructure.Hubs;
using FastBite.Shared.DTOS;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace FastBite.Presentation.Controllers;

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
            return StatusCode(500, new { message = "Internal Server Error", ex });
        }
    }
    
    [HttpPost("addProductsToCart")]
    public async Task<IActionResult> AddProductToCart([FromBody] CartRequestDTO request)
    {
        try 
        {
            if (request.PartyId.HasValue)
            {
                await _productService.AddProductToCartAsync(request.UserId, request.ProductId);
                await _hubContext.Clients.Group(request.PartyId.Value.ToString()).SendAsync("PartyCartUpdated", request.PartyId.Value);
                return Ok("Product added to party cart");
            }
            else
            {
                await _productService.AddProductToCartAsync(request.UserId, request.ProductId);
                await _hubContext.Clients.All.SendAsync("CartUpdated", request.UserId);
                return Ok("Product added to personal cart");
            }
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error adding product to cart", error = ex.Message });
        }
    }

    [HttpDelete("removeFromCart")]
    public async Task<IActionResult> RemoveProductFromCart([FromBody] CartRequestDTO request)
    {
        try
        {
            if (request.PartyId.HasValue)
            {
                await _productService.RemoveProductFromCartAsync(request.UserId, request.ProductId);
                await _hubContext.Clients.Group(request.PartyId.Value.ToString()).SendAsync("PartyCartUpdated", request.PartyId.Value);
                return Ok("Product removed from party cart");
            }
            else
            {
                await _productService.RemoveProductFromCartAsync(request.UserId, request.ProductId);
                await _hubContext.Clients.All.SendAsync("CartUpdated", request.UserId);
                return Ok("Product removed from personal cart");
            }
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error removing product from cart", error = ex.Message });
        }
    }

    [HttpDelete("clearCart")]
    public async Task<IActionResult> ClearCart([FromBody] CartClearRequestDTO request)
    {
        try
        {
            if (request.PartyId.HasValue)
            {
                await _productService.ClearCartAsync(request.UserId);
                await _hubContext.Clients.Group(request.PartyId.Value.ToString()).SendAsync("PartyCartUpdated", request.PartyId.Value);
                return Ok("Party cart cleared");
            }
            else
            {
                await _productService.ClearCartAsync(request.UserId);
                await _hubContext.Clients.All.SendAsync("CartUpdated", request.UserId);
                return Ok("Personal cart cleared");
            }
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error clearing cart", error = ex.Message });
        }
    }
}