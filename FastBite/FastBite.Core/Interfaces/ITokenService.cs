using System.Security.Claims;
using FastBite.Core.Models;
using FastBite.Shared.DTOS;

namespace FastBite.Core.Interfaces;

public interface ITokenService
{
    Task<string> GenerateTokenAsync(User user);
    Task<string> GenerateRefreshTokenAsync();
    Task<string> GenerateEmailTokenAsync(string userId);
    ClaimsPrincipal GetPrincipalFromToken(string token, bool validateLifetime = true);
    Task ValidateEmailTokenAsync(string token);
} 