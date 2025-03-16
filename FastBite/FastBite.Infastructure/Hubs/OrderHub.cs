using Microsoft.AspNetCore.SignalR;
using System;
using System.Threading.Tasks;

namespace FastBite.Infastructure.Hubs;
public class CartHub : Hub
{
    public async Task NotifyCartUpdated(string userId)
    {
        await Clients.All.SendAsync("CartUpdated", userId);
    }

    public async Task JoinPartyGroup(Guid partyId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, partyId.ToString());
    }

    public async Task LeavePartyGroup(Guid partyId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, partyId.ToString());
    }

    public async Task NotifyPartyCartUpdated(Guid partyId)
    {
        await Clients.Group(partyId.ToString()).SendAsync("PartyCartUpdated", partyId);
    }
}