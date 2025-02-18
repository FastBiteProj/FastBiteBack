namespace FastBite.Core.Interfaces
{
    public interface ICheckoutService
    {
        public Task<string> GetPayPalAccessTokenAsync(string PayPalUrl, string PayPalClientId, string PayPalSecret);
        public Task<string> CreateOrderAsync(string PayPalUrl, string accessToken, decimal amount, string currency);
        public Task<string> CaptureOrderAsync(string PayPalUrl, string accessToken, string orderId);
    }
}