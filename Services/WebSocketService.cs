using Microsoft.AspNetCore.SignalR;

namespace Morcon.Services;

public class MorconHub : Hub
{
    public async Task SendMessage(string message)
    {
        await Clients.All.SendAsync("ReceiveMessage", message);
    }
}
public class WebSocketService
{

}
