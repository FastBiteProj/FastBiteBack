using FastBite.Core.Interfaces;
using FastBite.Shared.DTOS;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace FastBite.Implementation.Classes;
public class RecaptchaService : IRecaptchaService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly string _secretKey;

    public RecaptchaService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _secretKey = "6LdmgYEqAAAAAJCORePHgoyDPMPU1jJYcWHRAOBG";
    }

    public async Task<bool> ValidateRecaptcha(string recaptchaToken)
    {
        var client = _httpClientFactory.CreateClient();
        var response = await client.GetStringAsync(
            $"https://www.google.com/recaptcha/api/siteverify?secret={_secretKey}&response={recaptchaToken}");
        
        var recaptchaResult = JsonConvert.DeserializeObject<RecaptchaResponse>(response);

        return recaptchaResult.Success;
    }
}


