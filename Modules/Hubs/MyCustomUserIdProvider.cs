using System.Security.Claims;
using Microsoft.AspNetCore.SignalR;

namespace MessagerieInterneAPI.Modules.Hubs
{
    public class MyCustomUserIdProvider : IUserIdProvider
    {
        public string GetUserId(HubConnectionContext connection)
        {
            var userId = connection.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            Console.WriteLine("User ID utilisé par SignalR : " + userId);
            return userId;
        }

    }
}
