namespace FastBite.Shared.DTOS;

public class RemoveFromPartyCartDTO
{
    public Guid PartyId { get; set; }
    public Guid ProductId { get; set; }
}