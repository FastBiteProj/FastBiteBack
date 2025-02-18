public record RegisterDTO(
    string Name,
    string Surname,
    string Email,
    string PhoneNumber,
    string Password,
    string ConfirmPassword,
    string CaptchaToken
);
