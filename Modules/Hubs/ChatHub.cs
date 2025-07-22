using MessagerieInterneAPI.Data;
using MessagerieInterneAPI.Modules.Discussion;
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

        public async Task SendMessageToDiscussion(int idExp, int idDest, int? idGroupe, string message)
        {
            var msg = new MessageModel
            {
                Id_expediteur = idExp,
                Id_destinataire = idDest,
                Id_groupe_discussion = (int)idGroupe,
                Contenu = message,
                Date_envoi = DateTime.UtcNow,
                Id_statut_msg = 1
            };

            var liason = new Connexion().ConnectPostgres();
            new MessageService().SendMessage(liason, msg);

            if (idGroupe != null)
                await Clients.Group("groupe_" + idGroupe).SendAsync("ReceiveMessage", idExp, message);
            else
            {
                await Clients.User(idExp.ToString()).SendAsync("ReceiveMessage", idExp, message);
                await Clients.User(idDest.ToString()).SendAsync("ReceiveMessage", idExp, message);
            }
        }

    }
}
