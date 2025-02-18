using FastBite.Core.Models;
using FastBite.Shared.DTOS;
using FastBite.Shared.Exceptions;
using FastBite.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text;
using FastBite.Infrastructure.Contexts;
using static BCrypt.Net.BCrypt;
using Microsoft.IdentityModel.Tokens;

namespace FastBite.Implementation.Classes;

public class AccountService : IAccountService
{
    private readonly IEmailSender emailSender;
    private readonly ITokenService tokenService;
    private readonly FastBiteContext context; 
    private readonly IAuthService authService;

    public AccountService(IEmailSender emailSender, ITokenService tokenService, FastBiteContext context, IAuthService authService)
    {
        this.emailSender = emailSender;
        this.tokenService = tokenService;
        this.context = context;
        this.authService = authService;
    }


    public async Task ConfirmEmailAsync(string token)
    {
        var principal = tokenService.GetPrincipalFromToken(token, validateLifetime: true); 

        var email = principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;

        var user = context.Users.FirstOrDefault(u => u.Email == email);

        if (user == null)
        {
            throw new MyAuthException(AuthErrorTypes.UserNotFound, "User not found");
        }

        var confirmationToken = await tokenService.GenerateEmailTokenAsync(user.Id.ToString());

        var link = $"http://localhost:5156/api/v1/Account/ValidateConfirmation?token={confirmationToken}";

        
        StringBuilder sb = new( File.ReadAllText("/Users/mrknsol/Documents/FastBite0/FastBite-FastBite/FastBite/assets/email.html"));
        
        sb.Replace("[Confirmation Link]", link);
        sb.Replace("[Year]", DateTime.Now.Year.ToString());
        sb.Replace("[Recipient's Name]", user.Email);
        sb.Replace("[Your Company Name]", "JWT Identity");
        
        await emailSender.SendEmailAsync(user.Email, "Email confirmation", sb.ToString(), isHtml: true);
    }

    public async Task SendVerificationCodeAsync(ForgotPasswordDTO forgotPasswordDto)
    {
        var user = await context.Users.FirstOrDefaultAsync(u => u.Email == forgotPasswordDto.Email);

        if (user == null)
        {
            throw new MyAuthException(AuthErrorTypes.UserNotFound, "User not found");
        }

        var verificationCode = Functions.GenerateVerificationCode();
        
        user.PasswordResetCode = verificationCode;
        user.PasswordResetCodeExpiryTime = DateTime.UtcNow.AddMinutes(15);

        await context.SaveChangesAsync();

        StringBuilder sb = new StringBuilder(File.ReadAllText("/Users/mrknsol/Documents/FastBite//FastBite/FastBite/assets/codepage.html"));
        sb.Replace("[Verification Code]", verificationCode);
        sb.Replace("[Year]", DateTime.Now.Year.ToString());
        sb.Replace("[Recipient's Name]", user.Email);
        sb.Replace("[Your Company Name]", "FastBite");

        await emailSender.SendEmailAsync(user.Email, "Password Reset Verification Code", sb.ToString(), isHtml: true);
    }

    public async Task ResetPasswordWithCodeAsync(ResetPasswordWithCodeDTO resetPasswordDto)
    {
        var user = await context.Users.FirstOrDefaultAsync(u => 
            u.PasswordResetCode == resetPasswordDto.VerificationCode && 
            u.PasswordResetCodeExpiryTime > DateTime.UtcNow); 

        if (user == null)
        {
            throw new MyAuthException(AuthErrorTypes.InvalidCredentials, "Invalid or expired verification code");
        }

        if (resetPasswordDto.NewPassword != resetPasswordDto.ConfirmNewPassword)
        {
            throw new MyAuthException(AuthErrorTypes.PasswordMismatch, "Passwords do not match");
        }

        user.Password = HashPassword(resetPasswordDto.NewPassword);
        user.PasswordResetCode = null;
        user.PasswordResetCodeExpiryTime = null;

        await emailSender.SendEmailAsync(user.Email, "Password Reset Confirmation", "Your password has been successfully reset");

        await context.SaveChangesAsync();
    }

    public async Task<UserInfoDTO> GetUserInfoAsync(string token)
    {
        try 
        {
            var principal = tokenService.GetPrincipalFromToken(token, validateLifetime: false);
            var email = principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;

            var user = await context.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.AppRole)
                .FirstOrDefaultAsync(u => u.Email == email);
            
            if (user == null)
            {
                throw new MyAuthException(AuthErrorTypes.UserNotFound, "User not found");
            }

            return new UserInfoDTO(
                Id: user.Id,
                FirstName: user.Name,
                LastName: user.Surname,
                PhoneNumber: user.phoneNumber,
                Email: user.Email,
                AccessToken: token
            );
        }
        catch (SecurityTokenExpiredException) 
        {
            try 
            {
                var expiredPrincipal = tokenService.GetPrincipalFromToken(token, validateLifetime: false);
                var email = expiredPrincipal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;

                var user = await context.Users.FirstOrDefaultAsync(u => u.Email == email);
                
                if (user == null || user.RefreshTokenExpiryTime <= DateTime.Now)
                {
                    throw new MyAuthException(AuthErrorTypes.InvalidCredentials, "Session expired. Please login again");
                }

                var refreshResult = await authService.RefreshTokenAsync(new TokenDTO( 
                    AccessToken: token,
                    RefreshToken: user.RefreshToken) 
                );

                return new UserInfoDTO(
                    Id: user.Id,
                    FirstName: user.Name,
                    LastName: user.Surname,
                    PhoneNumber: user.phoneNumber,
                    Email: user.Email,
                    AccessToken: refreshResult.AccessToken
                );
            }
            catch 
            {
                throw new MyAuthException(AuthErrorTypes.InvalidCredentials, "Session expired. Please login again");
            }
        }
    }

    public async Task ResetPaswordAsync(ResetPasswordDTO resetRequest, string token)
    {
        var principal = tokenService.GetPrincipalFromToken(token, validateLifetime: true);

        var email = principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;

        var user = await context.Users.FirstOrDefaultAsync(u => u.Email == email);

        if (user == null)
        {
            throw new MyAuthException(AuthErrorTypes.UserNotFound, "User not found");
        }

        if (!Verify(resetRequest.OldPassword, user.Password))
        {
            throw new MyAuthException(AuthErrorTypes.InvalidCredentials, "Invalid credentials");
        }

        if (resetRequest.NewPassword != resetRequest.ConfirmNewPassword)
        {
            throw new MyAuthException(AuthErrorTypes.PasswordMismatch, "Passwords do not match");
        }
        
        user.Password = HashPassword(resetRequest.NewPassword);

        await emailSender.SendEmailAsync(user.Email, "Password Reset", "Your password has been reset");

        await context.SaveChangesAsync();
    }
    
    public async Task UpdateUserAsync(UpdateUserInfoDTO updateUserDto, string token)
    {
        var principal = tokenService.GetPrincipalFromToken(token, validateLifetime: true);
        var email = principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;

        var user = await context.Users.FirstOrDefaultAsync(u => u.Email == email);

        if (user == null)
        {
            throw new MyAuthException(AuthErrorTypes.UserNotFound, "User not found");
        }

        if (!string.IsNullOrWhiteSpace(updateUserDto.FirstName))
        {
            user.Name = updateUserDto.FirstName;
        }
        
        if (!string.IsNullOrWhiteSpace(updateUserDto.LastName))
        {
            user.Surname = updateUserDto.LastName;
        }
        
        if (!string.IsNullOrWhiteSpace(updateUserDto.PhoneNumber))
        {
            user.phoneNumber = updateUserDto.PhoneNumber;
        }
        if (!string.IsNullOrWhiteSpace(updateUserDto.Email))
        {
            user.Email = updateUserDto.Email;
        }

        await context.SaveChangesAsync();
        await emailSender.SendEmailAsync(user.Email, "Profile Update", "Your profile has been updated successfully");
    }

    public async Task<List<UserInfoDTO>> GetUsersAsync()
    {
        var users = await context.Users
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.AppRole)
            .Where(u => !u.UserRoles.Any(ur => ur.AppRole.Name == "AppAdmin"))
            .Select(u => new UserInfoDTO(
                u.Id,
                u.Name,
                u.Surname,
                u.phoneNumber,
                u.Email,
                null 
            ))
            .ToListAsync();

        return users;
    }

    public async Task<bool> DeleteUserAsync(Guid userId)
    {
        var user = await context.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.AppRole)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
            return false;

        if (user.UserRoles.Any(ur => ur.AppRole.Name == "AppAdmin"))
            throw new MyAuthException(AuthErrorTypes.InvalidCredentials, "Cannot delete admin user");

        context.Users.Remove(user);
        await context.SaveChangesAsync();
        
        return true;
    }

    public async Task<bool> UpdateUserByAdminAsync(Guid userId, UpdateUserInfoDTO updateUserDto)
    {
        var user = await context.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.AppRole)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
            return false;

        if (user.UserRoles.Any(ur => ur.AppRole.Name == "AppAdmin"))
            throw new MyAuthException(AuthErrorTypes.InvalidCredentials, "Cannot modify admin user");

        if (!string.IsNullOrWhiteSpace(updateUserDto.FirstName))
            user.Name = updateUserDto.FirstName;
        
        if (!string.IsNullOrWhiteSpace(updateUserDto.LastName))
            user.Surname = updateUserDto.LastName;
        
        if (!string.IsNullOrWhiteSpace(updateUserDto.PhoneNumber))
            user.phoneNumber = updateUserDto.PhoneNumber;
        
        if (!string.IsNullOrWhiteSpace(updateUserDto.Email))
            user.Email = updateUserDto.Email;

        await context.SaveChangesAsync();
        await emailSender.SendEmailAsync(user.Email, "Profile Update", "Your profile has been updated by administrator");
        
        return true;
    }

}
