using System.Security.AccessControl;

namespace FastBite.Shared.DTOS;

public class PartyDTO {
    public Guid PartyId { get; set; }
    public string PartyCode { get; set; }
    public int TableId { get; set; }
    public Guid OwnerId { get; set; }
    public List<Guid> MemberIds { get; set; }
    public List<Guid> OrderItems { get; set; } = new();
};