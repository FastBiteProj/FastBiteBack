using FastBite.Data.Contexts;
using FastBite.Data.Models;
using FastBite.Data.DTOS;
using FastBite.Exceptions;
using FastBite.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using static BCrypt.Net.BCrypt;
using AutoMapper;
using FastBite.Data.Configs;

namespace FastBite.Services.Classes;

public class AuthService : IAuthService
{
    private readonly FastBiteContext context;
    private readonly ITokenService tokenService;
    private readonly IBlackListService blackListService;
    private readonly Mapper mapper;
    public AuthService(FastBiteContext context, ITokenService tokenService, IBlackListService blackListService, IEmailSender emailSender)
    {
        this.context = context;
        this.tokenService = tokenService;
        this.blackListService = blackListService;
        mapper = MappingConfiguration.InitializeConfig();
    }

    public async Task<AccessInfoDTO> LoginUserAsync(LoginDTO user)
    {
        try
        {
            var foundUser = await context.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.AppRole)
                .FirstOrDefaultAsync(u => u.Email == user.Email);

            if (foundUser == null)
            {
                throw new MyAuthException(AuthErrorTypes.UserNotFound, "User not found");
            }

            if (!Verify(user.Password, foundUser.Password))
            {
                throw new MyAuthException(AuthErrorTypes.InvalidCredentials, "Invalid credentials");
            }

            var userRoles = foundUser.UserRoles.Select(ur => ur.AppRole.Name).ToList();
            
            string userRole = userRoles.Contains("AppAdmin") ? "AppAdmin" : "AppUser";

            var tokenData = new AccessInfoDTO(
                foundUser.Id.ToString(),
                foundUser.Name,
                foundUser.Email,
                await tokenService.GenerateTokenAsync(foundUser),
                await tokenService.GenerateRefreshTokenAsync(),
                DateTime.Now.AddMinutes(130),
                userRole
            );

            foundUser.RefreshToken = tokenData.RefreshToken;
            foundUser.RefreshTokenExpiryTime = tokenData.RefreshTokenExpireTime;

            await context.SaveChangesAsync();

            return tokenData;
        }
        catch
        {
            throw;
        }
    }

    public async Task LogOutAsync(TokenDTO userTokenInfo)
    {
        if (userTokenInfo is null)
            throw new MyAuthException(AuthErrorTypes.InvalidRequest, "Invalid client request");

        var principal = tokenService.GetPrincipalFromToken(userTokenInfo.AccessToken);

        var email = principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;

        var user = context.Users.FirstOrDefault(u => u.Email == email);

        user.RefreshToken = null;
        user.RefreshTokenExpiryTime = DateTime.Now;
    
        await context.SaveChangesAsync();

        blackListService.AddTokenToBlackList(userTokenInfo.AccessToken);
    }

    public async Task<AccessInfoDTO> RefreshTokenAsync(TokenDTO userAccessData)
    {
        // Проверка входных данных
        if (userAccessData is null || 
            string.IsNullOrEmpty(userAccessData.AccessToken) || 
            string.IsNullOrEmpty(userAccessData.RefreshToken))
        {
            throw new MyAuthException(AuthErrorTypes.InvalidRequest, "Invalid client request: missing token data");
        }

        var principal = tokenService.GetPrincipalFromToken(userAccessData.AccessToken);
        if (principal == null)
        {
            throw new MyAuthException(AuthErrorTypes.InvalidToken, "Invalid access token");
        }

        // Получение данных из claims
        var email = principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
        var name = principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(name))
        {
            throw new MyAuthException(AuthErrorTypes.InvalidToken, "Invalid token claims");
        }

        // Получение пользователя с ролями
        var user = await context.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.AppRole)
            .FirstOrDefaultAsync(u => u.Email == email);

        if (user == null)
        {
            throw new MyAuthException(AuthErrorTypes.UserNotFound, "User not found");
        }

        if (user.RefreshToken != userAccessData.RefreshToken)
        {
            throw new MyAuthException(AuthErrorTypes.InvalidToken, "Invalid refresh token");
        }

        if (user.RefreshTokenExpiryTime <= DateTime.Now)
        {
            throw new MyAuthException(AuthErrorTypes.TokenExpired, "Refresh token expired");
        }

        var userRoles = user.UserRoles.Select(ur => ur.AppRole.Name).ToList();
        string userRole = userRoles.Contains("AppAdmin") ? "AppAdmin" : "AppUser";

        var newAccessToken = await tokenService.GenerateTokenAsync(user);
        var newRefreshToken = await tokenService.GenerateRefreshTokenAsync();

        user.RefreshToken = newRefreshToken;
        user.RefreshTokenExpiryTime = DateTime.Now.AddDays(1);

        await context.SaveChangesAsync();

        return new AccessInfoDTO(
            user.Id.ToString(),
            name,
            email,
            newAccessToken,
            newRefreshToken,
            user.RefreshTokenExpiryTime,
            userRole
        );
    }

    public async Task<RegisterDTO> RegisterUserAsync(RegisterDTO user)
    {
        try
        {
            var role = await context.AppRoles.Where(x => x.Name == "AppUser").FirstOrDefaultAsync();

            if (role == null) {
                throw new Exception("Role not found");
            }
            
            var newUser = new User
            {
                Name = user.Name,
                Surname = user.Surname,
                Email = user.Email,
                Password = HashPassword(user.Password),
                phoneNumber = user.PhoneNumber
            };
            
            await context.Users.AddAsync(newUser);
            await context.SaveChangesAsync();

            var roleToApply = new UserRole()
            {
                RoleId = role.Id,
                UserId = newUser.Id
            };

            context.UserRoles.Add(roleToApply);
            await context.SaveChangesAsync();
            
            return mapper.Map<RegisterDTO>(newUser);
        }
        catch
        {
            throw;
        }
    }
}