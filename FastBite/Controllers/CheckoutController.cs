using FastBite.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FastBite.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class CheckoutController : ControllerBase {
    public string PayPalClientId { get; set; } = "";
    public string PayPalSecret { get; set; } = "";
    public string PayPalUrl { get; set; } = "";
    public ICheckoutService _checkoutService;

    public CheckoutController(IConfiguration configuration, ICheckoutService checkoutService)
    {
        PayPalClientId = configuration["PayPalSettings:ClientId"];
        PayPalSecret = configuration["PayPalSettings:Secret"];
        PayPalUrl = configuration["PayPalSettings:Url"];
        _checkoutService = checkoutService;
    }

    [HttpGet("GetPayPalAccessToken")]
    public async Task<IActionResult> GetPayPalAccessToken() {
        var accessToken = await _checkoutService.GetPayPalAccessTokenAsync(PayPalUrl, PayPalClientId, PayPalSecret);
        
        return Ok(accessToken);
    }

    [HttpPost("CreateOrder")]
    public async Task<IActionResult> CreateOrder([FromBody] OrderRequestDTO orderRequest)
    {
        var accessToken = await _checkoutService.GetPayPalAccessTokenAsync(PayPalUrl, PayPalClientId, PayPalSecret);
        var orderId = await _checkoutService.CreateOrderAsync(PayPalUrl, accessToken, orderRequest.Amount, orderRequest.Currency);
        
        return Ok(new { orderId });
    }

    [HttpPost("CaptureOrder")]
    public async Task<IActionResult> CaptureOrder(string orderId)
    {
        var accessToken = await _checkoutService.GetPayPalAccessTokenAsync(PayPalUrl, PayPalClientId, PayPalSecret);
        var captureId = await _checkoutService.CaptureOrderAsync(PayPalUrl, accessToken, orderId);
        
        return Ok(new { captureId });
    }
    [HttpGet("client-id")]
    public IActionResult GetClientId()
    {
        var clientId = PayPalClientId;
        if (string.IsNullOrEmpty(clientId))
        {
            return NotFound("PayPal Client ID not found.");
        }
        return Ok(new { clientId });
    }
}