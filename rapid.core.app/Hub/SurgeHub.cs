using Microsoft.AspNetCore.SignalR;

namespace rapid.core.app.Hub
{
    public class SurgeHub
    {
        public async Task Broadcast(object data)
        {
            //await Clients.All.SendAsync("Update", data);
        }
    }
}
