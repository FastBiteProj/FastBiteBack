namespace FastBite.Shared.DTOS;

public class AddToPartyCartRequestDTO
{
    public Guid PartyId { get; set; }
    public Guid ProductId { get; set; }
}