using FastBite.Data.DTOS;
using Microsoft.AspNetCore.Authorization;
using FastBite.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;

namespace AuthApiService.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class RoleController : ControllerBase
{

    private readonly IRoleService _roleService;

    public RoleController(IRoleService roleService)
    {
        _roleService = roleService;
    }

    [HttpGet("All")]
    public async Task<IActionResult> GetAllRolesAsync()
    {
        var res = await _roleService.GetAllRolesAsync();

        return Ok(res);
    }

    [HttpPost("New")]
    public async Task AddNewRoleAsync([FromBody] RoleDTO role)
    {
        await _roleService.AddNewRoleAsync(role);
    }
    
    [HttpPost("Grant")]
    public async Task GrantRoleAsync([FromBody] GrantRoleDTO roleDto)
    {
        await _roleService.GrantRoleAsync(roleDto);
    }

    [HttpDelete("DeleteRole")]
    public async Task DeleteRoleAsync(string roleName, string email)
    {
        await _roleService.DeleteRoleAsync(roleName, email);
    }

    
}