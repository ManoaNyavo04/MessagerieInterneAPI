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
        private readonly IConfiguration _config;
        private readonly EspaceTravailService _service;
        private readonly UtilisateurService _utilisateurService;
        private readonly Connexion _connexion;

        public EspaceTravailController(AppDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
            _context = context;
            _service = new EspaceTravailService(_context);
            _utilisateurService = new UtilisateurService(_context);
            _connexion = new Connexion();
        }

        [HttpGet("getEspacesByUtilisateurId")]
        public async Task<IActionResult> GetEspacesByUtilisateurId()
        {
            var idUtilisateurClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            if (idUtilisateurClaim == null) return Unauthorized();

            int idUtilisateur = int.Parse(idUtilisateurClaim.Value);
            Console.WriteLine("id ve hitany (message zone): " + idUtilisateur);

            /*var espaceActifClaim = User.Claims.FirstOrDefault(c => c.Type == "EspaceActif");
            if (espaceActifClaim == null) return BadRequest("Espace actif non défini");
            int espaceActif = int.Parse(espaceActifClaim.Value);*/


            var espaces = await _service.GetEspacesByUtilisateurIdAsync(idUtilisateur);
            return Ok(espaces);
        }

        [HttpGet("espace-actif")]
        public IActionResult GetEspaceActif()
        {
            var idEspaceClaim = User.Claims.FirstOrDefault(c => c.Type == "EspaceActifId")?.Value;
            var nomEspaceClaim = User.Claims.FirstOrDefault(c => c.Type == "EspaceActifNom")?.Value;

            if (idEspaceClaim == null)
                return BadRequest("Aucun espace actif défini.");

            return Ok(new
            {
                idEspace = idEspaceClaim,
                nomEspace = nomEspaceClaim
            });
        }

        [HttpPost("changerEspace")]
        public IActionResult ChangerEspace([FromBody] int nouvelEspace)
        {
            var idUtilisateurClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            if (idUtilisateurClaim == null) return Unauthorized();

            int idUtilisateur = int.Parse(idUtilisateurClaim.Value);

            // ⚙️ Récupère l'utilisateur depuis la base
            var utilisateur = _utilisateurService.GetUtilisateurId(idUtilisateur).Result;
            if (utilisateur == null) return NotFound();

            var nouvelEspaceTravail = _service.GetEspacesTravailIdAsync(nouvelEspace).Result;
            if (nouvelEspaceTravail == null) return NotFound();

            var espace = _service.GetEspacesByUtilisateurIdAsync(idUtilisateur);
            if (espace == null) return NotFound();

            // 🧠 Génère un nouveau token avec le nouvel espace
            var token = new LoginRequest().GenererToken(utilisateur, _config, nouvelEspaceTravail.Id_espace_travail, nouvelEspaceTravail.Nom);
            var profilUtilisateur = _utilisateurService.GetProfilUtilisateur(utilisateur);

            return Ok(new { token, profilUtilisateur });
        }

        [HttpPost("affecterUtilisateurVersEspaceTravail")]
        public async Task<IActionResult> AffecterUtilisateurVersEspaceTravail([FromBody] UtilisateurEspaceTravailModel model)
        {
            if (model == null)
                return BadRequest("Données invalides");
            var result = await _service.AffecterUtilisateurVersEspaceTravail(model);
            if (result == null)
            {
                return BadRequest("L'utilisateur est déjà affecté à cet espace de travail.");
            }
            return Ok(new { message = "Affectation réussie", result });
        }

        [HttpGet("getAllEspaceTravail")]
        public async Task<IActionResult> GetAllEspaceTravail()
        {
            var espaces = await _service.GetAllEspaceTravail();
            return Ok(espaces);
        }


    }
}