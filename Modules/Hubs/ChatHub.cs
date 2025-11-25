using MessagerieInterneAPI.Data;
using MessagerieInterneAPI.Modules.Discussion;
using Microsoft.AspNetCore.SignalR;

namespace MessagerieInterneAPI
{
    public class ChatHub : Hub
    {
        private readonly MessageService _messageService;

        private readonly GroupeDiscussionService _grpDiscuService;
        private readonly PieceJointService _pieceJointeService;
        private Connexion connexion = new Connexion();

        public ChatHub(MessageService messageService, GroupeDiscussionService grpDiscuService, PieceJointService pieceJointeService)
        {
            _messageService = messageService;
            _grpDiscuService = grpDiscuService;
            _pieceJointeService = pieceJointeService;
        }
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

        public async Task<object> SendMessageToDiscussion(int idExp, int? idDest, int? idGroupe, string message, string groupName, bool diffuser = true)
{
    if (idGroupe == null && idDest == null)
        throw new ArgumentException("Le message doit avoir un destinataire ou un groupe.");

    // 🧩 Récupérer id_espace_travail depuis les claims
    var idEspaceTravailClaim = Context.User?.Claims?.FirstOrDefault(c => c.Type == "EspaceActifId");
    int? idEspaceTravail = null;

    if (idEspaceTravailClaim != null && int.TryParse(idEspaceTravailClaim.Value, out var parsedId))
        idEspaceTravail = parsedId;
    else
        Console.WriteLine("⚠️ id_espace_travail non trouvé dans les claims !");

    // ✅ Création du message
    var msg = new MessageModel
    {
        Id_expediteur = idExp,
        Id_destinataire = idDest,
        Id_groupe_discussion = idGroupe,
        Contenu = message,
        Date_envoie = DateTime.UtcNow,
        Id_status_msg = 1,
        Id_espace_travail = idEspaceTravail
    };

    var insertedMessage = await _messageService.SendMessage(msg);

    string nomExpediteur = await _messageService.GetNomExpediteur(connexion.ConnectPostgres(), idExp, idDest ?? 0);

    var pieceJointe = await _pieceJointeService.GetPieceJointeParMessage(connexion.ConnectPostgres(), insertedMessage.Id_message);

    string nomFichier = null;
    string cheminFichier = null;

    if (pieceJointe != null && pieceJointe.Count > 0)
    {
        var premierFichier = pieceJointe[0];
        nomFichier = premierFichier.Nom_original;
        cheminFichier = premierFichier.Chemin;
    }

    var payload = new
    {
        id_message = insertedMessage.Id_message,
        id_discussion = idGroupe ?? idDest,
        id_expediteur = idExp,
        expediteur_nom = nomExpediteur,
        id_destinataire = idDest,
        id_groupe_discussion = idGroupe,
        contenu = message,
        date_envoie = DateTime.UtcNow.ToString("o"),
        est_lu = false,
        nom_fichier = nomFichier,
        chemin_fichier = cheminFichier
    };

    if (diffuser)
    {
        if (idGroupe != null)
        {
            await Clients.Group(groupName).SendAsync("ReceiveMessage", payload);

            var membres = await _grpDiscuService.GetMembresGroupe(connexion.ConnectPostgres(), idGroupe.Value);
            foreach (var membre in membres)
            {
                if (membre.Id_utilisateur != idExp)
                {
                    var unreadCounts = await _messageService.GetUnreadCounts(membre.Id_utilisateur);
                    await Clients.User(membre.Id_utilisateur.ToString()).SendAsync("UpdateUnreadCounts", unreadCounts);
                }
            }
        }
        else if (idDest != null)
        {
            await Clients.User(idDest.ToString()).SendAsync("ReceiveMessage", payload);
            await Clients.User(idExp.ToString()).SendAsync("ReceiveMessage", payload);

            var unreadCounts = await _messageService.GetUnreadCounts(idDest.Value);
            await Clients.User(idDest.Value.ToString()).SendAsync("UpdateUnreadCounts", unreadCounts);
        }
    }

    return payload;
}


        public async Task UpdateMessageWithFile(int idMessage, string nomFichier, string cheminFichier, int? idDest, int? idGroupe, string groupName)
        {
            var payload = new
            {
                id_message = idMessage,
                nom_fichier = nomFichier,
                chemin_fichier = cheminFichier
            };

            if (idGroupe != null)
            {
                await Clients.Group(groupName).SendAsync("UpdateMessage", payload);
            }
            else if (idDest != null)
            {
                await Clients.User(idDest.ToString()).SendAsync("UpdateMessage", payload);
            }
        }


        public async Task<object> ReenvoyerMessage(int idExp, int? idDest, int? idGroupe, string message, string groupName)
        {
            if (idGroupe == null && idDest == null)
                throw new ArgumentException("Le message doit avoir un destinataire ou un groupe.");

            var msg = new MessageModel
            {
                Id_expediteur = idExp,
                Id_destinataire = idDest,
                Id_groupe_discussion = idGroupe,
                Contenu = message,
                Date_envoie = DateTime.UtcNow,
                Id_status_msg = 1
            };

            // var liason = new Connexion().ConnectPostgres();
            // ✅ Insert en DB (connexion gérée en pool)
            var insertedMessage = await _messageService.SendMessage(msg);

            string nomExpediteur = await _messageService.GetNomExpediteur(connexion.ConnectPostgres(), idExp, idDest ?? 0);

            var pieceJointe = await _pieceJointeService.GetPieceJointeParMessage(connexion.ConnectPostgres(), insertedMessage.Id_message);

            var payload = new
            {
                id_message = insertedMessage.Id_message,
                id_discussion = idGroupe ?? idDest,
                id_expediteur = idExp,
                expediteur_nom = nomExpediteur,
                id_destinataire = idDest,
                id_groupe_discussion = idGroupe,
                contenu = message,
                date_envoie = DateTime.UtcNow.ToString("o"),
                est_lu = false,
                piece_jointe = pieceJointe?.Select(pj => pj.Chemin).ToList()
            };

            // ✅ Diffusion ciblée
            if (idGroupe != null)
            {
                await Clients.Group(groupName).SendAsync("ReceiveMessage", payload);

                var membres = await _grpDiscuService.GetMembresGroupe(connexion.ConnectPostgres(), idGroupe.Value);
                foreach (var membre in membres)
                {
                    if (membre.Id_utilisateur != idExp) // Ne pas envoyer le comptage à l'expéditeur
                    {
                        var unreadCounts = await _messageService.GetUnreadCounts(membre.Id_utilisateur); // tu l'as déjà
                        await Clients.User(membre.Id_utilisateur.ToString()).SendAsync("UpdateUnreadCounts", unreadCounts);
                    }
                }

            }
            else if (idDest != null)
            {
                await Clients.User(idDest.ToString()).SendAsync("ReceiveMessage", payload);
                await Clients.User(idExp.ToString()).SendAsync("ReceiveMessage", payload);

                var unreadCounts = await _messageService.GetUnreadCounts(idDest.Value); // tu l'as déjà
                await Clients.User(idDest.Value.ToString()).SendAsync("UpdateUnreadCounts", unreadCounts);
                // await Clients.User(idExp.ToString()).SendAsync("UpdateUnreadCounts", unreadCounts);
            }
            return payload;
        }


        public override Task OnConnectedAsync()
        {
            // Ici tu pourrais ajouter automatiquement l'utilisateur à ses groupes
            return base.OnConnectedAsync();
        }

        public async Task NotifyMessagesRead(int discussionId, int userId, int idsDesMessagesLus)
        {
            await Clients.Group($"discussion_{discussionId}")
                .SendAsync("MessagesRead", new { discussionId, userId, idsDesMessagesLus });
        }


        /*public async Task SendMessageToDiscussion(int idExp, int? idDest, int? idGroupe, string message, string groupName)
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
        }*/





    }
}
