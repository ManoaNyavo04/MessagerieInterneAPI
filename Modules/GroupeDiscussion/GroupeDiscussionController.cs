using System.Security.Claims;
using MessagerieInterneAPI.Data;
using MessagerieInterneAPI.Entite;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace MessagerieInterneAPI
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class GroupeDiscussionController : ControllerBase
    {
        private Connexion connexion = new Connexion();

        private readonly GroupeDiscussionService _service;

        public GroupeDiscussionController(GroupeDiscussionService service)
        {
            _service = service;
        }

        [HttpGet("mesGrpDiscu")]
        public async Task<IActionResult> GetGrpDiscuByUser()
        {
            var idUtilisateurClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            if (idUtilisateurClaim == null) return Unauthorized();

            int idUtilisateur = int.Parse(idUtilisateurClaim.Value);
            Console.WriteLine("id ve hitany: " + idUtilisateur);
            var liaisonBase = connexion.ConnectPostgres();
            var mesGrp = _service.GetUtilisateurGrpDiscu(liaisonBase, idUtilisateur);

            return Ok(mesGrp);
        }
    }
}
