using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace FastBite.Infastructure.Hubs;
public class CartHub : Hub
{
    public async Task NotifyCartUpdated(string userId)
    {
        await Clients.All.SendAsync("CartUpdated", userId);
    }
}