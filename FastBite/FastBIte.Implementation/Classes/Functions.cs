using Microsoft.EntityFrameworkCore;
using FastBite.Infrastructure.Contexts;
using FastBite.Shared.DTOS;
using System.Text;
using FastBite.Core.Models;
using FastBite.Infrastructure.Contexts;

namespace FastBite.Implementation.Classes;
public static class Functions
{
    public static async Task<IQueryable<T>> GetFilteredDataByUserRoleAsync<T>(
        User user, 
        IQueryable<T> query, 
        FastBiteContext _context 
    ) where T : class
    {
        var userRole = await _context.UserRoles
            .Where(ur => ur.UserId == user.Id)
            .Include(u => u.AppRole)
            .ToListAsync();

        if (!userRole.Any(u => u.AppRole.Name == "AppAdmin"))
        {
            query = query.Where(q => EF.Property<Guid>(q, "UserId") == user.Id);
        }

        return query;
    }

    public static string GenerateVerificationCode()
    {
        var random = new Random();
        var verificationCode = new StringBuilder();
        
        for (int i = 0; i < 4; i++)
        {
            verificationCode.Append(random.Next(0, 10)); 
        }

        return verificationCode.ToString();
    }
}
