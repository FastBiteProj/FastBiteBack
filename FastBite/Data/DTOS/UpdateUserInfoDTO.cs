namespace FastBite.Data.DTOS;

public record UpdateUserInfoDTO
(
    string FirstName, 
    string LastName,
    string Email,
    string PhoneNumber 
);
