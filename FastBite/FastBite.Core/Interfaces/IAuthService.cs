using FastBite.Core.Models;
using FastBite.Shared.DTOS;

namespace FastBite.Core.Interfaces;

public interface IAuthService
{
    public Task<AccessInfoDTO> LoginUserAsync(LoginDTO user);
    public Task<RegisterDTO> RegisterUserAsync(RegisterDTO user);
    public Task<AccessInfoDTO> RefreshTokenAsync(TokenDTO userAccessData);
    public Task LogOutAsync(TokenDTO userTokenInfo);


}
