using System.Security.Claims;
using MessagerieInterneAPI.Data;
using MessagerieInterneAPI.Entite;
using Microsoft.AspNetCore.Mvc;
using Npgsql;

namespace MessagerieInterneAPI.Modules.Utilisateur
{
    [ApiController]
    [Route("api/[controller]")]
    public class UtilisateurController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;
        private UtilisateurService _service;
        private EspaceTravailService _espaceService;
        private LoginRequest login = new LoginRequest();

        public UtilisateurController(IConfiguration config, AppDbContext context, UtilisateurService service, EspaceTravailService espaceService)
        {
            _config = config;
            _context = context;
            _service = service ?? throw new ArgumentNullException(nameof(service));
            _espaceService = espaceService ?? throw new ArgumentNullException(nameof(espaceService));
        }


        [HttpPost("testConnex")]
        public IActionResult TestConnexion()
        {
            Connexion connect = new Connexion();
            var connex = connect.ConnectPostgres();
            return Ok("ieeeee : " + connex);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest model)
        {
            var siUtilisateur = await _service.VerifUtilisateur(model.Matricule, model.Mdp);
            if (siUtilisateur == null)
            {
                return Unauthorized("Matricule ou mot de passe invalide eeeeeeeeee.");
            }

            // 🔹 Récupère tous les espaces liés à cet utilisateur
            var espacesTravail = await _espaceService.GetEspacesByUtilisateurIdAsync(siUtilisateur.Id_utilisateur);

            // 🔹 Sélectionne le premier espace comme espace actif par défaut
            int espaceActif = espacesTravail.FirstOrDefault()?.IdEspaceTravail ?? 0;

            var token = login.GenererToken(siUtilisateur, _config, espaceActif);
            var profilUtilisateur = _service.GetProfilUtilisateur(siUtilisateur);
            // var espacesTravail = await _espaceService.GetEspacesByUtilisateurIdAsync(siUtilisateur.Id_utilisateur);
            return Ok(new
            {
                token,
                profilUtilisateur,
                espacesTravail
            });

        }

        [HttpPost("changerEspace")]
        public async Task<IActionResult> ChangerEspaceAsync([FromBody] int nouvelEspace)
        {
            var idUtilisateurClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            if (idUtilisateurClaim == null) return Unauthorized();

            int idUtilisateur = int.Parse(idUtilisateurClaim.Value);

            // ⚙️ Récupère l'utilisateur depuis la base
            var utilisateur = await _service.GetUtilisateurById(idUtilisateur);
            if (utilisateur == null) return NotFound();

            // 🧠 Génère un nouveau token avec le nouvel espace
            var token = new LoginRequest().GenererToken(utilisateur, _config, nouvelEspace);
            var profilUtilisateur = _service.GetProfilUtilisateur(utilisateur);

            return Ok(new { token, profilUtilisateur });
        }


        [HttpGet("allUsers")]

        public async Task<IActionResult> GetAllUtilisateurs()
        {


            Connexion connect = new Connexion();
            var connex = connect.ConnectPostgres();
            var result = _service.GetAllUtilisateurs(connex);
            return Ok(result);
        }

        [HttpPost("addUtilisateur")]
        public async Task<IActionResult> AddUtilisateur([FromBody] UtilisateurModel utilisateur)
        {
            var connexion = new Connexion().ConnectPostgres();
            var result = await _service.VerifMatricule(connexion, utilisateur);
            if (!result)
            {
                return BadRequest("Matricule déjà utilisé.");
            }
            return Ok(new { result, message = "Utilisateur ajouté avec succès." });
        }

        [HttpGet("searchUser")]
        public async Task<IActionResult> SearchUser([FromQuery] string searchTerm, [FromQuery] int idEspaceTravail)
        {
            Connexion connect = new Connexion();
            var connex = connect.ConnectPostgres();
            var result = _service.SearchUtilisateur(connex, searchTerm, idEspaceTravail);
            return Ok(result);
        }

        [HttpGet("allUserApi")]
        public async Task<IActionResult> GetUtilisateurApi()
        {
            var employes = await _service.GetAllEmployesApi();
            return Ok(employes);
        }

        [HttpPost("rafraichir")]
        public async Task<IActionResult> RafraichirUtilisateurs()
        {
            await _service.SynchroniserUtilisateursAsync();
            return Ok(new { message = "Synchronisation terminée avec succès." });
        }
    }
}

