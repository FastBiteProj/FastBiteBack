namespace FastBite.Shared.DTOS;
public class OrderReceiptItemDTO
{
    public string ProductName { get; set; }
    public int Quantity { get; set; }
    public decimal Price { get; set; }   
    public decimal Total => Price * Quantity;
}