using System.Net.Http;
using System.Threading.Tasks;
using FastBite.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace FastBite.Services.Classes;
public class RecaptchaService : IRecaptchaService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly string _secretKey;

    public RecaptchaService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _secretKey = configuration["Recaptcha:SecretKey"];
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


