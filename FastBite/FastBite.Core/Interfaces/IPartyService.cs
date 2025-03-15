using FastBite.Shared.DTOS;

namespace FastBite.Core.Interfaces;

public interface IPartyService
{
    public Task<Guid> CreatePartyAsync(Guid ownerId, int tableId);
    public Task<string> JoinPartyAsync(string partyCode, Guid userId);
    public Task<bool> LeavePartyAsync(Guid partyId, Guid userId);
    public Task<PartyDTO?> GetPartyAsync(Guid partyId);
    public Task<List<ProductDTO>> GetPartyCartAsync(Guid partyId);
    public Task AddProductToPartyCartAsync(Guid partyId, Guid productId);
    public Task RemoveProductFromPartyCartAsync(Guid partyId, Guid productId);
    public Task ClearPartyCartAsync(Guid partyId);
}