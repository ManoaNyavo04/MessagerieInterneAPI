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
            Console.WriteLine($"👥 JoinGroup() appelé pour : {groupName}, ConnId: {Context.ConnectionId}");
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        }


        public async Task SendMessageToGroup(string groupName, string user, string message)
        {
            await Clients.Group(groupName).SendAsync("ReceiveMessage", user, message);
        }

        public async Task SendMessageToDiscussion(int idExp, int? idDest, int? idGroupe, string message, string groupName)
        {
            try
            {
                Console.WriteLine($"📥 Reçu dans Hub : idExp={idExp}, idDest={idDest}, idGroupe={idGroupe}, message={message}");

                if (idGroupe == null && idDest == null)
                {
                    throw new ArgumentException("Le message doit avoir un destinataire ou un groupe.");
                }


                var msg = new MessageModel
                {
                    Id_expediteur = idExp,
                    Id_destinataire = idDest,
                    Id_groupe_discussion = idGroupe,
                    Contenu = message,
                    Date_envoie = DateTime.UtcNow,
                    Id_status_msg = 1
                };

                var liason = new Connexion().ConnectPostgres();
                await new MessageService().SendMessage(liason, msg);
                
                 var payload = new
                    {
                        id_expediteur = idExp,
                        id_destinataire = idDest,
                        contenu = message,
                        date_envoie = DateTime.UtcNow.ToString("o")
                    };
                await Clients.All.SendAsync("ReceiveMessage", payload);
                await Clients.All.SendAsync("ReceiveMessage", payload);

                // ✅ Logique de diffusion correct selon le type
                if (idGroupe != null)
                {
                    Console.WriteLine($"📤 Envoi au groupe {groupName} (ID: {idGroupe})");
                    await Clients.Group(groupName).SendAsync("ReceiveMessage", new
                    {
                        id_expediteur = idExp,
                        id_groupe_discussion = idGroupe,
                        contenu = message,
                        date_envoie = DateTime.UtcNow.ToString("o")
                    });
                    

                }
                else
                {
                    Console.WriteLine($"📤 Envoi aux utilisateurs {idExp} et {idDest}");


                    await Clients.User(idExp.ToString()).SendAsync("ReceiveMessage", payload);
                    await Clients.User(idDest.ToString()).SendAsync("ReceiveMessage", payload);

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ ERREUR DANS HUB : " + ex.Message);
                Console.WriteLine(ex.StackTrace);
                throw;
            }
    }





    }
}
