using FastBite.Data.DTOS;

namespace FastBite.Services.Interfaces;

public interface IRoleService
{
    public Task<IEnumerable<RoleDTO>> GetAllRolesAsync();
    public Task GrantRoleAsync(GrantRoleDTO roleDto);
    public Task AddNewRoleAsync(RoleDTO role);
    public Task DeleteRoleAsync(string roleName, string phoneNumber);
}