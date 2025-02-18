namespace FastBite.Shared.DTOS;
public record RecaptchaResponse
(
    bool Success, 
    string Hostname, 
    string ChallengeTimestamp, 
    string[] ErrorCodes
);