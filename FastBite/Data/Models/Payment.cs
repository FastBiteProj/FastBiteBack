// FastBite/Data/Models/Payment.cs
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FastBite.Data.Models
{
    public class Payment
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid? OrderId { get; set; }
        public Order? Order { get; set; }

        public Guid? ReservationId { get; set; }
        public Reservation? Reservation { get; set; }

        public string PayPalOrderId { get; set; }

        public string? PayPalCaptureId { get; set; }

        public decimal Amount { get; set; }

        public string Currency { get; set; }

        public string Status { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}