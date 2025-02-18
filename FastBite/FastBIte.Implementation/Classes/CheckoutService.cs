using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using FastBite.Core.Interfaces;
using Newtonsoft.Json;

namespace FastBite.Implementation.Classes
{
    public class CheckoutService : ICheckoutService
    {
        public async Task<string> GetPayPalAccessTokenAsync(string PayPalUrl, string PayPalClientId, string PayPalSecret)
        {
            string accessToken = "";

            string url = $"{PayPalUrl}/v1/oauth2/token";
            using (var client = new HttpClient())
            {
                string credentials64 = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{PayPalClientId}:{PayPalSecret}"));

                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials64);

                var requestMessage = new HttpRequestMessage(HttpMethod.Post, url);
                requestMessage.Content = new StringContent("grant_type=client_credentials", Encoding.UTF8, "application/x-www-form-urlencoded");

                var response = await client.SendAsync(requestMessage);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var jsonResponse = JsonConvert.DeserializeObject<dynamic>(responseContent);

                    if (jsonResponse != null)
                    {
                        accessToken = jsonResponse["access_token"].ToString();
                    }
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new Exception($"Ошибка при получении access token: {errorContent}");
                }
            }

            return accessToken;
        }

        public async Task<string> CreateOrderAsync(string PayPalUrl, string accessToken, decimal amount, string currency)
        {
            string orderId = "";

            string url = $"{PayPalUrl}/v2/checkout/orders";
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                var order = new
                {
                    intent = "CAPTURE",
                    purchase_units = new[]
                    {
                        new
                        {
                            amount = new
                            {
                                currency_code = currency,
                                value = amount.ToString("F2")
                            }
                        }
                    },
                    application_context = new
                    {
                        return_url = "https://yourdomain.com/return",
                        cancel_url = "https://yourdomain.com/cancel"
                    }
                };

                var jsonOrder = JsonConvert.SerializeObject(order);
                var content = new StringContent(jsonOrder, Encoding.UTF8, "application/json");

                var response = await client.PostAsync(url, content);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var jsonResponse = JsonConvert.DeserializeObject<dynamic>(responseContent);
                    orderId = jsonResponse["id"].ToString();
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new Exception($"Error while creating order: {errorContent}");
                }
            }

            return orderId;
        }

        public async Task<string> CaptureOrderAsync(string PayPalUrl, string accessToken, string orderId)
        {
            string captureId = "";

            string url = $"{PayPalUrl}/v2/checkout/orders/{orderId}/capture";
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                var response = await client.PostAsync(url, null);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var jsonResponse = JsonConvert.DeserializeObject<dynamic>(responseContent);
                    captureId = jsonResponse["purchase_units"][0]["payments"]["captures"][0]["id"].ToString();
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new Exception($"Error while capturing order: {errorContent}");
                }
            }

            return captureId;
        }
    }
}