namespace FastBite.Data.DTOS;

public record ResetPasswordDTO
(
     string OldPassword, 
     string NewPassword ,
     string ConfirmNewPassword 
);
 