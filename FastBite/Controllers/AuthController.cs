using FastBite.Data.Contexts;
using FastBite.Data.DTOS;
using FastBite.Exceptions;
using FastBite.Services.Classes;
using FastBite.Services.Interfaces;
using FastBite.Validators;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;

namespace FastBite.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class AuthController : ControllerBase
{
    private readonly LoginUserValidator loginValidator;
    private readonly RegisterUserValidator registerValidator;
    private readonly IAuthService authService;
    private readonly IRecaptchaService recaptchaService;

    public AuthController(LoginUserValidator loginValidator, RegisterUserValidator registerValidator, IAuthService authService, IRecaptchaService recaptchaService)
    {
        this.loginValidator = loginValidator;
        this.registerValidator = registerValidator;
        this.authService = authService;
        this.recaptchaService = recaptchaService;
    }

    [HttpPost("Login")]
    public async Task<IActionResult> LoginAsync([FromBody] LoginDTO user)
    {
        var validationResult = loginValidator.Validate(user);

        if (!validationResult.IsValid)
        {
            return BadRequest(validationResult.Errors);
        }

        try
        {
            var res = await authService.LoginUserAsync(user);
            
            var cookieOptions = new CookieOptions {
                HttpOnly = true,
                Secure = false,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddMinutes(15)
            };

            Response.Cookies.Append("accessToken", res.AccessToken, cookieOptions);
            Response.Cookies.Append("refreshToken", res.RefreshToken, cookieOptions);

            return Ok(res);
        }
        catch (MyAuthException ex)
        {
            return BadRequest($"{ex.Message}\n{ex.AuthErrorType}");
        }
    }

    [HttpPost("Register")]
    public async Task<IActionResult> RegisterAsync([FromBody] RegisterDTO user)
    {
        try
        {
            var isValidRecaptcha = await recaptchaService.ValidateRecaptcha(user.CaptchaToken); 
            
            if (!isValidRecaptcha) 
            { 
                return BadRequest("Failed reCAPTCHA verification"); 
            }

            var validationResult = registerValidator.Validate(user);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors);
            }
            var res = await authService.RegisterUserAsync(user);
            return Ok(res);
        }
        catch (MyAuthException ex)
        {
            return BadRequest($"{ex.Message}\n{ex.AuthErrorType}");
        }
    }


   [HttpPost("Refresh")]
    public async Task<IActionResult> RefreshTokenAsync()
    {
        try
        {
            var accessToken = Request.Cookies["accessToken"];
            var refreshToken = Request.Cookies["refreshToken"];

            if (string.IsNullOrEmpty(accessToken) || string.IsNullOrEmpty(refreshToken))
            {
                return BadRequest("Tokens not found in cookies");
            }

            var tokenDto = new TokenDTO
            (
                AccessToken: accessToken,
                RefreshToken: refreshToken
            );

            var newTokens = await authService.RefreshTokenAsync(tokenDto);

            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Lax,
                Expires = DateTime.UtcNow.AddMinutes(15)
            };

            Response.Cookies.Append("accessToken", newTokens.AccessToken, cookieOptions);
            Response.Cookies.Append("refreshToken", newTokens.RefreshToken, cookieOptions);

            var responseData = new
            {
                accessToken = newTokens.AccessToken,
                refreshToken = newTokens.RefreshToken,
                role = newTokens.Role
            };

            return Ok(responseData);
        }
        catch (MyAuthException ex)
        {
            Response.Cookies.Delete("accessToken");
            Response.Cookies.Delete("refreshToken");
            return BadRequest($"{ex.Message}\n{ex.AuthErrorType}");
        }
        catch (Exception ex)
        {
            Response.Cookies.Delete("accessToken");
            Response.Cookies.Delete("refreshToken");
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }


    [Authorize]
    [HttpPost("Logout")]
    public async Task<IActionResult> LogoutAsync()
    {
        try
        {
            var accessToken = Request.Cookies["accessToken"];
            var refreshToken = Request.Cookies["refreshToken"];

            if (string.IsNullOrEmpty(accessToken) || string.IsNullOrEmpty(refreshToken))
            {
                return BadRequest("Tokens not found in cookies");
            }

            var tokenDto = new TokenDTO(accessToken, refreshToken);

            await authService.LogOutAsync(tokenDto);

            Response.Cookies.Delete("accessToken");
            Response.Cookies.Delete("refreshToken");

            return Ok("Logged out successfully");
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    

}
