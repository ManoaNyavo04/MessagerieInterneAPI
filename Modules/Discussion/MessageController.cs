using System.Security.Claims;
using MessagerieInterneAPI.Data;
using MessagerieInterneAPI.Entite;
using Microsoft.AspNetCore.Mvc;

namespace MessagerieInterneAPI.Modules.Discussion
{
    [ApiController]
    [Route("api/[controller]")]
    public class MessageController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly MessageService _service;
        private Connexion connexion = new Connexion();

        public MessageController(AppDbContext context)
        {
            _context = context;
            _service = new MessageService();
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

        /*[HttpPost("sendMessage")]
        public async Task<IActionResult> SendMessage(MessageModel message)
        {
            _service.SendMessage(connexion.ConnectPostgres(), message);
            return Ok("Message sent successfully.");
        }
        /*
        var discussions = await GetGroupes(idUtilisateur, conn);
discussions.AddRange(await GetPrives(idUtilisateur, conn));
return Ok(discussions);

        */
        [HttpGet("getMesDiscussions")]
        public async Task<IActionResult> GetMesDiscussions()
        {
            var idUtilisateurClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            if (idUtilisateurClaim == null) return Unauthorized();

            int idUtilisateur = int.Parse(idUtilisateurClaim.Value);
            Console.WriteLine("id ve hitany (disussion): " + idUtilisateur);
            var conn = connexion.ConnectPostgres();
            if (conn == null)
            {
                return StatusCode(500, "Database connection failed.");

            }


            var discussions = new List<DiscussionModel>();
            discussions.AddRange(_service.GetGrpDiscussionByUser(idUtilisateur, conn));
            discussions.AddRange(_service.GetDiscussionIndividuelleByUser(idUtilisateur, conn));

            return Ok(discussions);
        }

        [HttpGet("messages")]
        public async Task<IActionResult> GetMessages(int targetId, string type)
        {
            var idUtilisateurClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            if (idUtilisateurClaim == null) return Unauthorized();

            int idUtilisateur = int.Parse(idUtilisateurClaim.Value);
            Console.WriteLine("id ve hitany (message zone): " + idUtilisateur);


            List<MessageModel> messages = type == "groupe"
                ? _service.GetMessagesByGroupId(connexion.ConnectPostgres(), targetId)
                : _service.GetIndividualMessage(connexion.ConnectPostgres(), idUtilisateur, targetId);
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
            var conn = connexion.ConnectPostgres();
            if (conn == null)
            {
                return StatusCode(500, "Database connection failed.");

            }

            var discussion = _service.VerifOuCreeDiscussionIndividuelle(conn, idUtilisateur, model);

            return Ok(discussion);
        }
    }
}

