namespace FastBite.Shared.DTOS;

public record UpdateUserInfoDTO
(
    string FirstName, 
    string LastName,
    string Email,
    string PhoneNumber 
);
