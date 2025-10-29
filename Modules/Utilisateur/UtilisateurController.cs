using MessagerieInterneAPI.Data;
using MessagerieInterneAPI.Entite;
using Microsoft.AspNetCore.Authorization;
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

        /*[HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest model)
        {
            var siUtilisateur = await _service.VerifUtilisateur(model.Matricule, model.Mdp);
            if (siUtilisateur == null)
            {
                return Unauthorized("Matricule ou mot de passe invalide eeeeeeeeee.");
            }

            var token = login.GenererToken(siUtilisateur, _config);
            var profilUtilisateur = _service.GetProfilUtilisateur(siUtilisateur);
            return Ok(new
            {
                token,
                profilUtilisateur
            });

        }*/

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest model)
        {
            var siUtilisateur = await _service.VerifUtilisateur(model.Matricule, model.Mdp);
            if (siUtilisateur == null)
            {
                return Unauthorized("Matricule ou mot de passe invalide.");
            }

            // 1️⃣ Récupérer les espaces de travail
            var espaces = await _espaceService.GetEspacesByUtilisateurIdAsync(siUtilisateur.Id_utilisateur);
            int? idPremierEspace = espaces.FirstOrDefault()?.IdEspaceTravail;
            string? nomPremierEspace = espaces.FirstOrDefault()?.EspaceTravail;

            // 2️⃣ Générer le token avec l’espace actif
            var token = model.GenererToken(siUtilisateur, _config, idPremierEspace, nomPremierEspace);

            var profilUtilisateur = _service.GetProfilUtilisateur(siUtilisateur);

            return Ok(new
            {
                token,
                profilUtilisateur,
                espaceActif = new
                {
                    idEspaceTravail = idPremierEspace,
                    nomEspaceTravail = nomPremierEspace
                },
                espacesDisponibles = espaces
            });
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

        [Authorize]
        [HttpGet("searchUser")]
        public async Task<IActionResult> SearchUser([FromQuery] string searchTerm)
        {
            var idEspaceClaim = User.Claims.FirstOrDefault(c => c.Type == "EspaceActifId")?.Value;
            var nomEspaceClaim = User.Claims.FirstOrDefault(c => c.Type == "EspaceActifNom")?.Value;

            if (idEspaceClaim == null)
                return BadRequest("Aucun espace actif défini.");

            Console.WriteLine("Espace Actif ID depuis le token : " + idEspaceClaim);
            Connexion connect = new Connexion();
            var connex = connect.ConnectPostgres();
            var result = _service.SearchUtilisateur(connex, searchTerm, int.Parse(idEspaceClaim));
            return Ok(result);
        }
    }
}

