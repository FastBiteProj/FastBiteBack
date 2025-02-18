using System.Runtime.InteropServices.ComTypes;
using FastBite.Data.Configs;
using FastBite.Data.Contexts;
using FastBite.Data.DTOS;
using FastBite.Data.Models;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using FastBite.Services.Interfaces;

namespace UserService.Classes;

public class RoleService : IRoleService
{
    private readonly Mapper _mapper;
    private readonly FastBiteContext _context;

    public RoleService(FastBiteContext context)
    {
        _context = context;
        _mapper = MappingConfiguration.InitializeConfig();
    }

    public async Task<IEnumerable<RoleDTO>> GetAllRolesAsync()
    {
        try
        {
            var roles = await _context.AppRoles.ToListAsync();
            return _mapper.Map<List<AppRole>, List<RoleDTO>>(roles);
        }
        catch
        {
            throw;
        }
    }

    public async Task AddNewRoleAsync(RoleDTO role)
    {
        try
        {
            var roleToAdd = _mapper.Map<RoleDTO, AppRole>(role);
            await _context.AppRoles.AddAsync(roleToAdd);
            await _context.SaveChangesAsync();
        }
        catch
        {
            throw;
        }
    }

    public async Task GrantRoleAsync(GrantRoleDTO roleDto)
    {
        var user = _context.Users.FirstOrDefault(x => x.Email == roleDto.email);
        var role = _context.AppRoles.FirstOrDefault(x => x.Name.ToLower() == roleDto.roleName.ToLower());

        var newUserRole = new UserRole()
        {
            RoleId = role.Id,
            UserId = user.Id
        };

        await _context.UserRoles.AddAsync(newUserRole);

        await _context.SaveChangesAsync();

    } 

    public async Task DeleteRoleAsync(string roleName, string email) {

        var role = await _context.AppRoles.FirstOrDefaultAsync(r => r.Name == roleName);

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

        var userRole = await _context.UserRoles.FirstOrDefaultAsync(u => u.RoleId == role.Id && u.UserId == user.Id);

        _context.UserRoles.Remove(userRole);
        _context.SaveChangesAsync();
    }
}


