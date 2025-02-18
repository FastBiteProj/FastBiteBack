using FastBite.Shared.DTOS;

namespace FastBite.Core.Interfaces;

public interface IRoleService
{
    public Task<IEnumerable<RoleDTO>> GetAllRolesAsync();
    public Task GrantRoleAsync(GrantRoleDTO roleDto);
    public Task AddNewRoleAsync(RoleDTO role);
    public Task DeleteRoleAsync(string roleName, string phoneNumber);
}