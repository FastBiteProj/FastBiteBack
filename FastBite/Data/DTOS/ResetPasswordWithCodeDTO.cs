public record ResetPasswordWithCodeDTO
(
    string VerificationCode, 
    string NewPassword, 
    string ConfirmNewPassword 
);