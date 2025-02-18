namespace FastBite.Services.Interfaces;

public interface IRecaptchaService {

    public Task<bool> ValidateRecaptcha(string recaptchaToken);
}