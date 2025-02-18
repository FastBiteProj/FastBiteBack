using FastBite.Data.Models;
using FastBite.Data.DTOS;

namespace FastBite.Services.Interfaces;

public interface IAuthService
{
    public Task<AccessInfoDTO> LoginUserAsync(LoginDTO user);
    public Task<RegisterDTO> RegisterUserAsync(RegisterDTO user);
    public Task<AccessInfoDTO> RefreshTokenAsync(TokenDTO userAccessData);
    public Task LogOutAsync(TokenDTO userTokenInfo);


}
