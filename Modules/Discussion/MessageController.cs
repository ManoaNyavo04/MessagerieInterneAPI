using MessagerieInterneAPI.Data;
using Microsoft.AspNetCore.Mvc;

namespace MessagerieInterneAPI.Modules.Discussion
{
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

        [HttpPost("sendMessage")]
        public async Task<IActionResult> SendMessage(MessageModel message)
        {
            _service.SendMessage(connexion.ConnectPostgres(), message);
            return Ok("Message sent successfully.");
        }

    }
}
