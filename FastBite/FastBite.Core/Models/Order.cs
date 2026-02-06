using System.ComponentModel.DataAnnotations;
using FastBite.Shared.Enum;

namespace FastBite.Core.Models
{
    public class Order
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public ICollection<OrderItem> OrderItems { get; set; }

        public int TotalPrice { get; set; }

        public DateTime ConfirmationDate { get; set; }
        
        [Timestamp]
        public byte[] RowVersion { get; set;}
        public Guid UserId { get; set; }

        public User User { get; set; }

        public int TableNumber { get; set; }
        public OrderStatus Status { get; set; } = OrderStatus.Created;

    }
}
