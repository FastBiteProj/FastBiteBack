using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;


namespace FastBite.Presentation.Controllers;
[ApiController]
[Route("api/v1/[controller]")]
public class RecaptchaController : ControllerBase
{
    private readonly IConfiguration _configuration;

    public RecaptchaController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    [HttpGet("SiteKey")]
    public IActionResult GetSiteKey()
    {
        var siteKey = _configuration["Recaptcha:SiteKey"];
        if (string.IsNullOrEmpty(siteKey))
        {
            return NotFound("Site Key not found.");
        }

        return Ok(new { siteKey });
    }
}
