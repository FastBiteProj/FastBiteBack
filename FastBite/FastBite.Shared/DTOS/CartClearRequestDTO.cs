namespace FastBite.Shared.DTOS;
public class CartClearRequestDTO
{
    public string UserId { get; set; }
    public Guid? PartyId { get; set; }
}