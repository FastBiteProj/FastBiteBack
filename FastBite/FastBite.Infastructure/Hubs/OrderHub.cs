using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace FastBite.Infrastructure.Hubs
{
    public class OrderHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            Console.WriteLine("🚀 Соединение установлено");
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            Console.WriteLine("❌ Соединение разорвано: " + exception?.Message);
            await base.OnDisconnectedAsync(exception);
        }
    }
}