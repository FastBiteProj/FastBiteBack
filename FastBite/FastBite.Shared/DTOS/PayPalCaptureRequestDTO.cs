namespace FastBite.Shared.DTOS
{
    public class PayPalCaptureRequestDTO
        {
            public Guid OrderId { get; set; }
            public string PayPalOrderId { get; set; }
            public string Language { get; set; }
        }
}