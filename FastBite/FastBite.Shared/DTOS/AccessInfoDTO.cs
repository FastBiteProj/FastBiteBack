namespace FastBite.Shared.DTOS;

public record AccessInfoDTO
(
     string id,
     string name,
     string email,
     string AccessToken ,
     string RefreshToken ,
     DateTime RefreshTokenExpireTime,
     string Role
);
