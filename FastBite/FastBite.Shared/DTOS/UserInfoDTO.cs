namespace FastBite.Shared.DTOS
{
    public record UserInfoDTO
    (
         Guid Id,
         string FirstName,
         string LastName,
         string PhoneNumber,
         string Email,
         string AccessToken
    );
}