namespace FastBite.Shared.DTOS;
public class CartRequestDTO
{
    public string UserId { get; set; }
    public Guid ProductId { get; set; }
    public Guid? PartyId { get; set; }
}