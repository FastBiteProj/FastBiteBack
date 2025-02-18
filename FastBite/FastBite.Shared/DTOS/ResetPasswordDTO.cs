namespace FastBite.Shared.DTOS;

public record ResetPasswordDTO
(
     string OldPassword, 
     string NewPassword ,
     string ConfirmNewPassword 
);
 