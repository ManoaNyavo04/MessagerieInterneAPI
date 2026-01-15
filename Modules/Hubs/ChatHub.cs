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
            try
            {
                if (idGroupe == null && idDest == null)
                    throw new ArgumentException("Le message doit avoir un destinataire ou un groupe.");

                var idEspaceTravailClaim = Context.User?.Claims?.FirstOrDefault(c => c.Type == "EspaceActifId");
                int? idEspaceTravail = idEspaceTravailClaim != null && int.TryParse(idEspaceTravailClaim.Value, out var parsedId)
                    ? parsedId
                    : null;

                Console.WriteLine("EspaceTravailId depuis le Hub : " + idEspaceTravail);

                var msg = new MessageModel
                {
                    Id_expediteur = idExp,
                    Id_destinataire = idDest,
                    Id_groupe_discussion = idGroupe,
                    Contenu = message ?? "", // ⇐ important
                    Date_envoie = DateTime.UtcNow,
                    Id_status_msg = 1,
                    Id_espace_travail = idEspaceTravail
                };

                var insertedMessage = await _messageService.SendMessage(msg);

                using var db = new Connexion().ConnectPostgres();
                db.Open();

                string nomExpediteur = await _messageService.GetNomExpediteur(db, idExp, idDest ?? 0);

                var pieceJointe = await _pieceJointeService.GetPieceJointeParMessage(db, insertedMessage.Id_message);

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
                    date_envoie = insertedMessage.Date_envoie.ToString("o"),
                    id_status_msg = 1,
                    modifiable_jusqua = insertedMessage.Modifiable_jusqua?.ToString("o"),
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
                        // await Clients.User(idExp.ToString()).SendAsync("ReceiveMessage", payload);

                        var unreadCounts = await _messageService.GetUnreadCounts(idDest.Value);
                        await Clients.User(idDest.Value.ToString()).SendAsync("UpdateUnreadCounts", unreadCounts);
                    }
                }

                return payload;
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ ERREUR DANS HUB : " + ex.Message);
                Console.WriteLine(ex.StackTrace);
                throw;
            }
        }


        public async Task UpdateMessageWithFile(
    int idMessage,
    string nomFichier,
    string cheminFichier,
    int? idDest,
    int? idGroupe,
    string groupName)
        {
            if (idGroupe == null && idDest == null)
                throw new ArgumentException("idDest ou idGroupe doit être fourni.");

            var payload = new
            {
                id_message = idMessage,
                nom_fichier = nomFichier,
                chemin_fichier = cheminFichier
            };

            if (idGroupe != null)
            {
                if (string.IsNullOrWhiteSpace(groupName))
                    throw new ArgumentException("groupName ne peut pas être vide pour les groupes.");

                await Clients.Group(groupName).SendAsync("UpdateMessage", payload);
            }
            else if (idDest != null)
            {
                // Diffusion aux deux utilisateurs (expéditeur + destinataire)
                await Clients.User(idDest.ToString()).SendAsync("UpdateMessage", payload);

                var idExp = Context.UserIdentifier; // id connecté
                if (idExp != null)
                {
                    await Clients.User(idExp).SendAsync("UpdateMessage", payload);
                }
            }
        }

        public async Task<object> UpdateMessageContent(
            int idMessage,
            string newContent,
            string groupName)
        {
            try
            {
                using var db = new Connexion().ConnectPostgres();
                db.Open();

                // 1️⃣ Charger l'ancien message
                var oldMsg = await _messageService.GetMessageById(db, idMessage);
                if (oldMsg == null)
                    throw new Exception("Message introuvable.");

                // 2️⃣ Vérifier le droit de modifier (si tu veux ajouter un délai)
                var now = DateTime.UtcNow;
                if (oldMsg.Modifiable_jusqua != null && now > oldMsg.Modifiable_jusqua)
                    throw new Exception("Délai de modification dépassé.");

                // 3️⃣ Mise à jour en base
                await _messageService.UpdateMessageContent(idMessage, newContent, 6);

                // 4️⃣ Recharger message après update
                var updatedMsg = await _messageService.GetMessageById(db, idMessage);

                // 5️⃣ Construire le payload EXACT comme dans SendMessage
                var payload = new
                {
                    id_message = updatedMsg.Id_message,
                    id_discussion = updatedMsg.Id_groupe_discussion ?? updatedMsg.Id_destinataire,
                    id_expediteur = updatedMsg.Id_expediteur,
                    expediteur_nom = updatedMsg.Nom_expediteur,
                    id_destinataire = updatedMsg.Id_destinataire,
                    id_groupe_discussion = updatedMsg.Id_groupe_discussion,
                    contenu = updatedMsg.Contenu,
                    date_envoie = updatedMsg.Date_envoie.ToString("o"),
                    Id_status_msg = 6, // statuts modifié
                    date_modification = updatedMsg.Date_modification?.ToString("o"),
                    est_lu = false,
                    nom_fichier = updatedMsg.Nom_original,
                    chemin_fichier = updatedMsg.Chemin
                };

                // 6️⃣ DIFFUSION
                if (updatedMsg.Id_groupe_discussion != null)
                {
                    await Clients.Group(groupName).SendAsync("MessageUpdated", payload);

                    var membres = await _grpDiscuService.GetMembresGroupe(db, updatedMsg.Id_groupe_discussion.Value);
                    foreach (var membre in membres)
                    {
                        if (membre.Id_utilisateur != updatedMsg.Id_expediteur)
                        {
                            var unreadCounts = await _messageService.GetUnreadCounts(membre.Id_utilisateur);
                            await Clients.User(membre.Id_utilisateur.ToString())
                                .SendAsync("UpdateUnreadCounts", unreadCounts);
                        }
                    }
                }
                else
                {
                    // conversation privée
                    await Clients.User(updatedMsg.Id_destinataire.ToString()).SendAsync("MessageUpdated", payload);
                    await Clients.User(updatedMsg.Id_expediteur.ToString()).SendAsync("MessageUpdated", payload);

                    var unreadCounts = await _messageService.GetUnreadCounts(updatedMsg.Id_destinataire.Value);
                    await Clients.User(updatedMsg.Id_destinataire.Value.ToString())
                        .SendAsync("UpdateUnreadCounts", unreadCounts);
                }

                return payload;
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ ERREUR UPDATE HUB : " + ex.Message);
                Console.WriteLine(ex.StackTrace);
                throw;
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





    }
}
