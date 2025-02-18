using FastBite.Shared.DTOS;
using FastBite.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FastBite.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductController(IProductService productService)
    {
        _productService = productService;
    }

    [Authorize(Roles = "AppAdmin")]
    [HttpPost("Create")]
    public async Task<IActionResult> CreateProduct([FromBody] ProductDTO productDto, CancellationToken cancellationToken)
    {
        try
        {
            var createdProduct = await _productService.AddNewProductAsync(productDto, cancellationToken);
            return Ok(createdProduct);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"InternalServerError: {ex.Message}");
        }
    }

    [HttpPost("upload")]
    public async Task<IActionResult> UploadImage(IFormFile file, CancellationToken cancellationToken)
    {
        try
        {
            var imageUrl = await _productService.UploadImageAsync(file, cancellationToken);
            return Ok(imageUrl); 
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("Get")]
    public async Task<IActionResult> GetAllProducts()
    {
        try
        {
            var products = await _productService.GetAllProductsAsync();
            return Ok(products);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"InternalServerError: {ex.Message}");
        }
    }

    [Authorize(Roles = "AppAdmin")]
    [HttpDelete("Delete")]
    public async Task<IActionResult> DeleteProduct([FromQuery] string productName) 
    {
        try
        {
            var res = await _productService.DeleteProductAsync(productName);
            return Ok(res);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [Authorize(Roles = "AppAdmin")]
    [HttpPut("Update")]
    public async Task<IActionResult> UpdateProduct([FromQuery] string productName, [FromBody] ProductDTO productDto)
    {
        try
        {
            Console.WriteLine(HttpContext.Request.Headers["accessToken"]);
            var res = await _productService.UpdateProductAsync(productName, productDto);
            return Ok(res);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}