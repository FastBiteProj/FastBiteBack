namespace FastBite.Shared.DTOS;
public class CartDTO
{
    public List<ProductDTO> Items { get; set; } = new List<ProductDTO>();
    public DateTime? ExpirationTime { get; set; }
}