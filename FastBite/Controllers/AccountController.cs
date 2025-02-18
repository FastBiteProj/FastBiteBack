using FastBite.Data.DTOS;
using FastBite.Exceptions;
using FastBite.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;
using FastBite.Validators;

namespace FastBite.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly IAccountService accountService;
        private readonly ITokenService tokenService;
        private readonly ResetPasswordValidator resetPasswordValidator;

        public AccountController(IAccountService accountService, ITokenService tokenService, ResetPasswordValidator resetPasswordValidator)
        {
            this.accountService = accountService;
            this.tokenService = tokenService;
            this.resetPasswordValidator = resetPasswordValidator;
        }

        [Authorize]
        [HttpPost("ResetPassword")]
        public async Task<IActionResult> ResetPasswordAsync([FromBody] ResetPasswordDTO resetRequest)
        {
            try
            {
                var token = HttpContext.Request.Cookies["accessToken"];

                token = token.ToString().Replace("Bearer ", "");

                await accountService.ResetPaswordAsync(resetRequest, token);
                return Ok("Recovery link sent to your email");
            }
            catch (MyAuthException ex)
            {
                return BadRequest($"{ex.Message}\n{ex.AuthErrorType}");
            }
        }
        [Authorize]
        [HttpPost("ConfirmEmail")]
        public async Task<IActionResult> ConfirmEmailAsync()
        {
            try
            {
                var token = HttpContext.Request.Cookies["accessToken"];
                token = token.ToString().Replace("Bearer ", "");

                await accountService.ConfirmEmailAsync(token);

                return Ok();
            }
            catch
            {
                throw;
            }
        }
        
        [Authorize]
        [HttpGet("UserInfo")]        
        public async Task<IActionResult> GetUserInfoAsync()
        {
            try
            {
                var token = HttpContext.Request.Cookies["accessToken"];

                var userInfo = await accountService.GetUserInfoAsync(token);

                if (userInfo == null)
                {
                    return NotFound("User not found.");
                }

                return Ok(userInfo);
            }
            catch (MyAuthException ex)
            {
                return BadRequest($"{ex.Message}\n{ex.AuthErrorType}");
            }
        }
        [HttpPost("SendVerificationCode")]
        public async Task<IActionResult> SendVerificationCode([FromBody] ForgotPasswordDTO request)
        {
            try
            {
                await accountService.SendVerificationCodeAsync(request);
                return Ok("Verification code sent to your email.");
            }
            catch (MyAuthException ex)
            {
                return BadRequest($"{ex.Message}\n{ex.AuthErrorType}");
            }
        }
        [HttpPost("ResetPasswordWithCode")]
        public async Task<IActionResult> ResetPasswordWithCode([FromBody] ResetPasswordWithCodeDTO resetRequest)
        {
            try
            {
                var validationResult = resetPasswordValidator.Validate(resetRequest);

                if (!validationResult.IsValid)
                {
                    return BadRequest(validationResult.Errors);
                }

                await accountService.ResetPasswordWithCodeAsync(resetRequest);
                return Ok(new { message = "Password has been reset successfully." });
            }
            catch (MyAuthException ex)
            {
                return BadRequest($"{ex.Message}\n{ex.AuthErrorType}");
            }
        }

        [HttpGet("ValidateConfirmation")]
        public async Task<IActionResult> ValidateConfirmationAsync([FromQuery] string token)
        {
            try
            {
                 await tokenService.ValidateEmailTokenAsync(token);

                return Ok("Email confirmed successfully");
            }
            catch
            {
                throw;
            }
        }
        [Authorize] 
        [HttpPut("UpdateUser")]
        public async Task<IActionResult> UpdateUser([FromBody] UpdateUserInfoDTO updateUserDto)
        {
            try
            {
                var token = HttpContext.Request.Cookies["accessToken"];

                token = token.ToString().Replace("Bearer ", "");

                await accountService.UpdateUserAsync(updateUserDto, token);
                return Ok("Confirmation message sent to your email");
            }
            catch (MyAuthException ex)
            {
                return BadRequest($"{ex.Message}\n{ex.AuthErrorType}");
            }
        }

        [Authorize(Roles = "AppAdmin")]
        [HttpGet("GetUsers")]
        public async Task<IActionResult> GetUsers()
        {
            try
            {
                var users = await accountService.GetUsersAsync();
                return Ok(users);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error");
            }
        }

        [Authorize(Roles = "AppAdmin")]
        [HttpDelete("DeleteUser")]
        public async Task<IActionResult> DeleteUser([FromQuery] Guid userId)
        {
            try
            {
                var result = await accountService.DeleteUserAsync(userId);
                if (result)
                {
                    return Ok("User successfully deleted");
                }
                return NotFound("User not found");
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error");
            }
        }

        [Authorize(Roles = "AppAdmin")]
        [HttpPut("UpdateUserByAdmin")]
        public async Task<IActionResult> UpdateUserByAdmin([FromQuery] Guid userId, [FromBody] UpdateUserInfoDTO updateUserDto)
        {
            try
            {
                var result = await accountService.UpdateUserByAdminAsync(userId, updateUserDto);
                if (result)
                {
                    return Ok("User successfully updated");
                }
                return NotFound("User not found");
            }
            catch (MyAuthException ex)
            {
                return BadRequest($"{ex.Message}\n{ex.AuthErrorType}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
    