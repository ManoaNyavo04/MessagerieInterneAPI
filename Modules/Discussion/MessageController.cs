using System.Security.Claims;
using MessagerieInterneAPI.Data;
using MessagerieInterneAPI.Entite;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace MessagerieInterneAPI.Modules.Discussion
{
    [ApiController]
    [Route("api/[controller]")]
    public class MessageController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly MessageService _service;
        private Connexion connexion = new Connexion();
        private readonly IHubContext<ChatHub> _hubContext;


        public MessageController(AppDbContext context, IHubContext<ChatHub> hubContext)
        {
            _context = context;
            _service = new MessageService();
            _hubContext = hubContext;
        }

        [HttpGet("getMessagesByGroup/{discussionId}")]
        public async Task<IActionResult> GetMessageByGroup(int discussionId)
        {
            var messages = _service.GetMessagesByGroupId(connexion.ConnectPostgres(), discussionId);
            if (messages == null || !messages.Any())
            {
                return NotFound("No messages found for this discussion.");
            }
            return Ok(messages);
        }

        [HttpGet("getMessagesByUser")]
        public async Task<IActionResult> GetIndividualMessage(int exp, int dest)
        {
            var messages = _service.GetIndividualMessage(connexion.ConnectPostgres(), exp, dest);
            if (messages == null || !messages.Any())
            {
                return NotFound("No messages found between the specified users.");
            }
            return Ok(messages);
        }

        [HttpGet("getMesDiscussions")]
        public async Task<IActionResult> GetMesDiscussions()
        {
            var idUtilisateurClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            if (idUtilisateurClaim == null) return Unauthorized();

            int idUtilisateur = int.Parse(idUtilisateurClaim.Value);

            Console.WriteLine("id ve hitany (disussion): " + idUtilisateur);
            var espaceClaim = User.Claims.FirstOrDefault(c => c.Type == "EspaceActifId")?.Value;
            var idEspaceActif = espaceClaim != null ? int.Parse(espaceClaim) : 0;
            var conn = connexion.ConnectPostgres();
            if (conn == null)
            {
                return StatusCode(500, "Database connection failed.");

            }


            var discussions = new List<DiscussionModel>();
            discussions.AddRange(_service.GetGrpDiscussionByUser(idUtilisateur,idEspaceActif, conn));
            discussions.AddRange(_service.GetDiscussionIndividuelleByUser(idUtilisateur, idEspaceActif, conn));

            return Ok(discussions);
        }

        [Authorize]
        [HttpGet("messages")]
        public async Task<IActionResult> GetMessages(int targetId, string type)
        {
            var idUtilisateurClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            if (idUtilisateurClaim == null) return Unauthorized();

            int idUtilisateur = int.Parse(idUtilisateurClaim.Value);
            Console.WriteLine("id ve hitany (message zone): " + idUtilisateur);

            Console.WriteLine($"🔎 API /messages → targetId: {targetId}, type: {type}");

            var messages = await _service.GetMessages(idUtilisateur, targetId, type);

            /*List<MessageModel> messages = type == "groupe"
                ? _service.GetMessagesByGroupId(connexion.ConnectPostgres(), targetId)
                : _service.GetIndividualMessage(connexion.ConnectPostgres(), idUtilisateur, targetId);*/
            Console.WriteLine("tafiditra??");

            return Ok(messages);
        }

        [HttpPost("demarrerDiscussion")]
        public async Task<IActionResult> CreateNewDiscussion([FromBody] DiscussionModel model)
        {
            var idUtilisateurClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            if (idUtilisateurClaim == null) return Unauthorized();
            int idUtilisateur = int.Parse(idUtilisateurClaim.Value);

            Console.WriteLine("id ve hitany (demarrer discussion): " + idUtilisateur);

            var espaceClaim = User.Claims.FirstOrDefault(c => c.Type == "EspaceActifId")?.Value;
            var idEspaceActif = espaceClaim != null ? int.Parse(espaceClaim) : 0;
            var conn = connexion.ConnectPostgres();
            if (conn == null)
            {
                return StatusCode(500, "Database connection failed.");

            }

            var discussion = _service.VerifOuCreeDiscussionIndividuelle(conn, idEspaceActif, idUtilisateur, model);

            return Ok(discussion);
        }

        [HttpGet("messagesNonLus")]
        public async Task<IActionResult> GetUnreadCounts()
        {
            var idUtilisateurClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            if (idUtilisateurClaim == null) return Unauthorized();

            int idUtilisateur = int.Parse(idUtilisateurClaim.Value);
            // Console.WriteLine("id ve hitany (message zone): " + idUtilisateur);

            var unreadCounts = await _service.GetUnreadCounts(idUtilisateur);
            Console.WriteLine("excuterrrrr" + idUtilisateur);
            return Ok(unreadCounts);
        }

        [HttpPut("lireMessage")]
        public async Task<IActionResult> MarkMessagesAsRead(int discussionId, string type)
        {
            var idUtilisateurClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            if (idUtilisateurClaim == null) return Unauthorized();


            int idUtilisateur = int.Parse(idUtilisateurClaim.Value);
            Console.WriteLine("id ve hitany (message zone): " + idUtilisateur);

            await _service.MarkMessagesAsRead(idUtilisateur, discussionId, type);

            // 🔁 Mise à jour en temps réel
            var updatedCounts = await _service.GetUnreadCounts(idUtilisateur);
            await _hubContext.Clients.User(idUtilisateur.ToString())
                .SendAsync("UpdateUnreadCounts", updatedCounts);
            await _hubContext.Clients.Group($"discussion_{discussionId}")
                .SendAsync("MessagesRead", new { discussionId, userId = idUtilisateur });

            return Ok();
        }

        [Authorize]
        [HttpGet("searchUserGroup")]
        public async Task<IActionResult> SearchUtilisateurEtGroupe([FromQuery] string searchTerm)
        {
            var idUtilisateurClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (idUtilisateurClaim == null) return Unauthorized();

            var idEspaceClaim = User.Claims.FirstOrDefault(c => c.Type == "EspaceActifId")?.Value;
            var nomEspaceClaim = User.Claims.FirstOrDefault(c => c.Type == "EspaceActifNom")?.Value;

            if (idEspaceClaim == null)
                return BadRequest("Aucun espace actif défini.");

            int idUtilisateur = int.Parse(idUtilisateurClaim);
            int idEspaceActif = int.Parse(idEspaceClaim);

            Console.WriteLine($"Utilisateur : {idUtilisateur}, Espace actif : {idEspaceActif}");

            var results = _service.SearchUtilisateurEtGroupe(connexion.ConnectPostgres(), idUtilisateur, searchTerm, idEspaceActif);
            return Ok(results);
        }

    }
}

