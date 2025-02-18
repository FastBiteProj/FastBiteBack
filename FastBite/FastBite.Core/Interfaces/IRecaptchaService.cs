using FastBite.Shared.DTOS;

namespace FastBite.Core.Interfaces;

public interface IRecaptchaService {

    public Task<bool> ValidateRecaptcha(string recaptchaToken);
}