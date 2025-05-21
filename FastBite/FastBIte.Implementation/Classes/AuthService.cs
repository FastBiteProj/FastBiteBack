using FastBite.Infrastructure.Contexts;
using FastBite.Core.Models;
using FastBite.Shared.DTOS;
using FastBite.Shared.Exceptions;
using FastBite.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using static BCrypt.Net.BCrypt;
using AutoMapper;
using FastBite.Implementation.Configs;
using StackExchange.Redis;

namespace FastBite.Implementation.Classes;

public class AuthService : IAuthService
{
    private readonly FastBiteContext context;
    private readonly ITokenService tokenService;
    private readonly IBlackListService blackListService;
    private readonly IMapper mapper;
    private readonly IConnectionMultiplexer _redis;
    private readonly IDatabase _db;

    public AuthService(FastBiteContext context, ITokenService tokenService, IBlackListService blackListService, IConnectionMultiplexer redis)
    {
        this.context = context;
        this.tokenService = tokenService;
        this.blackListService = blackListService;
        mapper = MappingConfiguration.InitializeConfig();
        _redis = redis;
        _db = _redis.GetDatabase();
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

            var cartKey = $"cart:{foundUser.Id}";
            var productIds = await _db.ListRangeAsync(cartKey);
            var cartProducts = new List<ProductDTO>();

            foreach (var productId in productIds)
            {
                var product = await context.Products
                    .Include(p => p.Category)
                    .Include(p => p.Translations)
                    .FirstOrDefaultAsync(p => p.Id == Guid.Parse(productId));

                if (product != null)
                {
                    cartProducts.Add(mapper.Map<ProductDTO>(product));
                }
            }

            var tokenData = new AccessInfoDTO(
                foundUser.Id.ToString(),
                foundUser.Name,
                foundUser.Email,
                await tokenService.GenerateTokenAsync(foundUser),
                await tokenService.GenerateRefreshTokenAsync(),
                DateTime.Now.AddMinutes(5),
                userRole,
                cartProducts
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
        if (userAccessData is null)
            throw new MyAuthException(AuthErrorTypes.InvalidRequest, "Invalid client request");

        var accessToken = userAccessData.AccessToken;
        var refreshToken = userAccessData.RefreshToken;

        var principal = tokenService.GetPrincipalFromToken(accessToken);
        var email = principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;

        var user = await context.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user == null || user.RefreshToken != refreshToken || user.RefreshTokenExpiryTime <= DateTime.Now)
            throw new MyAuthException(AuthErrorTypes.InvalidRequest, "Invalid client request");

        var userRole = await context.UserRoles
            .Include(r => r.AppRole)
            .FirstOrDefaultAsync(r => r.UserId == user.Id);

        var newAccessToken = await tokenService.GenerateTokenAsync(user);
        var newRefreshToken = await tokenService.GenerateRefreshTokenAsync();

        user.RefreshToken = newRefreshToken;
        user.RefreshTokenExpiryTime = DateTime.Now.AddDays(1);

        await context.SaveChangesAsync();

        var cartKey = $"cart:{user.Id}";
        var productIds = await _db.ListRangeAsync(cartKey);
        var cartProducts = new List<ProductDTO>();

        foreach (var productId in productIds)
        {
            var product = await context.Products
                .Include(p => p.Category)
                .Include(p => p.Translations)
                .FirstOrDefaultAsync(p => p.Id == Guid.Parse(productId));

            if (product != null)
            {
                cartProducts.Add(mapper.Map<ProductDTO>(product));
            }
        }

        return new AccessInfoDTO(
            user.Id.ToString(),
            user.Name,
            user.Email,
            newAccessToken,
            newRefreshToken,
            DateTime.Now.AddMinutes(5),
            userRole.AppRole.Name,
            cartProducts 
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