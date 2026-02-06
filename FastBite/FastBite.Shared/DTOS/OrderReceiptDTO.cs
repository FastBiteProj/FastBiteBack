namespace FastBite.Shared.DTOS;

public class OrderReceiptDTO
{
    public Guid OrderId { get; set; }
    public int TableNumber { get; set; }
    public DateTime ConfirmationDate { get; set; }
    public decimal TotalPrice { get; set; }

    public List<OrderReceiptItemDTO> Items { get; set; }
}
