using FastBite.Core.Interfaces;
using FastBite.Shared.DTOS;
using FastBite.Core.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using FastBite.Shared.Enum;

namespace FastBite.Presentation.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class CheckoutController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly ICheckoutService _checkoutService;

        public string PayPalClientId { get; set; }
        public string PayPalSecret { get; set; }
        public string PayPalUrl { get; set; }

        public CheckoutController(IOrderService orderService, ICheckoutService checkoutService, IConfiguration configuration)
        {
            _orderService = orderService;
            _checkoutService = checkoutService;

            PayPalClientId = configuration["PayPalSettings:ClientId"];
            PayPalSecret = configuration["PayPalSettings:Secret"];
            PayPalUrl = configuration["PayPalSettings:Url"];
        }

        [HttpGet("GetPayPalAccessToken")]
        public async Task<IActionResult> GetPayPalAccessToken() {
            var accessToken = await _checkoutService.GetPayPalAccessTokenAsync(PayPalUrl, PayPalClientId, PayPalSecret);
            
            return Ok(accessToken);
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

        [HttpPost("Pay")]
        public async Task<IActionResult> Pay([FromBody] PaymentRequestDTO request)
        {
            try
            {
                switch (request.PaymentMethod.ToLower())
                {
                    case "cash":
                    case "card":
                    {
                        var receipt = await _orderService.TryLockAndPayOrderAsync(
                            request.OrderId,
                            OrderStatus.Paid,
                            request.Language
                        );

                        return Ok(new
                        {
                            status = "Paid",
                            method = request.PaymentMethod,
                            order = new
                            {
                                items = receipt.Items.Select(i => new
                                {
                                    name = i.ProductName,
                                    quantity = i.Quantity,
                                    price = i.Price
                                }),
                                totalPrice = receipt.TotalPrice
                            }
                        });
                    }
                    default:
                        return BadRequest("Unknown payment method");
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ToString());
            }
        }

        [HttpPost("Cancel")]
        public async Task<IActionResult> CancelOrder([FromBody] CancelDTO request)
        {
            try
            {
                await _orderService.DeleteOrderAsync(request.OrderId);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);  
            }
        }

        [HttpPost("CapturePayPal")]
        public async Task<IActionResult> CapturePayPal([FromBody] PayPalCaptureRequestDTO request)
        {
            var accessToken = await _checkoutService.GetPayPalAccessTokenAsync(
                PayPalUrl, PayPalClientId, PayPalSecret);

            var captureId = await _checkoutService.CaptureOrderAsync(
                PayPalUrl, accessToken, request.PayPalOrderId);

            var orderDto = await _orderService.TryLockAndPayOrderAsync(
                request.OrderId,
                OrderStatus.Paid,
                request.Language
            );

            return Ok(new
            {
                CaptureId = captureId,
                Status = "Paid",
                order = new
                {
                    items = orderDto.Items.Select(i => new
                    {
                        name = i.ProductName,
                        quantity = i.Quantity,
                        price = i.Price
                    }),
                    totalPrice = orderDto.TotalPrice
                }
            });
        }

        [HttpGet("ActiveOrder")]
        public async Task<IActionResult> GetActiveOrder([FromBody] Guid userId)
        {
            var order = await _orderService.GetActiveOrderAsync(userId);

            if (order == null)
                return NotFound();

            return Ok(order);
        }
    }
}