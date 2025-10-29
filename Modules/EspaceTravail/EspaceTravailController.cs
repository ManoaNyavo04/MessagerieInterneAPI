using System.Security.Claims;
using MessagerieInterneAPI.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MessagerieInterneAPI
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class EspaceTravailController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly EspaceTravailService _service;
        private readonly Connexion _connexion;

        public EspaceTravailController(AppDbContext context)
        {
            _context = context;
            _service = new EspaceTravailService(_context);
            _connexion = new Connexion();
        }

        [HttpGet("getEspacesByUtilisateurId")]
        public async Task<IActionResult> GetEspacesByUtilisateurId()
        {
            var idUtilisateurClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            if (idUtilisateurClaim == null) return Unauthorized();

            int idUtilisateur = int.Parse(idUtilisateurClaim.Value);
            Console.WriteLine("id ve hitany (message zone): " + idUtilisateur);

            var espaceActifClaim = User.Claims.FirstOrDefault(c => c.Type == "EspaceActif");
            if (espaceActifClaim == null) return BadRequest("Espace actif non défini");
            int espaceActif = int.Parse(espaceActifClaim.Value);


            var espaces = await _service.GetEspacesByUtilisateurIdAsync(idUtilisateur);
            return Ok(espaces);
        }
    }
}
