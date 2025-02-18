using FastBite.Data.DTOS;

namespace FastBite.Services.Interfaces;

public interface IAccountService
{
    public Task ResetPaswordAsync(ResetPasswordDTO resetRequest, string token);
    public Task<UserInfoDTO> GetUserInfoAsync(string token);
    public Task SendVerificationCodeAsync(ForgotPasswordDTO forgotPasswordDTO);
    public Task ResetPasswordWithCodeAsync(ResetPasswordWithCodeDTO resetRequest);
    public Task ConfirmEmailAsync(string token);
    public Task UpdateUserAsync(UpdateUserInfoDTO updateUserDto, string token);
    public Task<List<UserInfoDTO>> GetUsersAsync();
    public Task<bool> DeleteUserAsync(Guid userId);
    public Task<bool> UpdateUserByAdminAsync(Guid userId, UpdateUserInfoDTO updateUserDto);
}
