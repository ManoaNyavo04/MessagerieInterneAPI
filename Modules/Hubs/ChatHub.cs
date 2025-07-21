using Microsoft.AspNetCore.SignalR;

namespace MessagerieInterneAPI
{
    public class ChatHub : Hub
    {
        public async Task SendMessage(string user, string message)
        {
            // Envoie le message à tous les clients connectés
            await Clients.All.SendAsync("ReceiveMessage", user, message);
        }

        public async Task JoinGroup(string groupName)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        }

        public async Task SendMessageToGroup(string groupName, string user, string message)
        {
            await Clients.Group(groupName).SendAsync("ReceiveMessage", user, message);
        }
    }
}
