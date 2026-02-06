namespace FastBite.Shared.DTOS;

public class PaymentRequestDTO
    {
        public Guid OrderId { get; set; }

        public decimal Amount { get; set; }

        public string Currency { get; set; }

        public string PaymentMethod { get; set; }
        public string Language { get; set; }
    }