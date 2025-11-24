using System.Security.Claims;
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
                // espaceActif = new
                // {
                //     idEspaceTravail = idPremierEspace,
                //     nomEspaceTravail = nomPremierEspace
                // },
                // espacesDisponibles = espaces

            });
        }

        // [Authorize(Roles = "1")]
        [Authorize(Roles = "m_1")]
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
            Console.WriteLine("Claims reçues :");
            foreach (var c in User.Claims)
            {
                Console.WriteLine($"Type = {c.Type}  |  Value = {c.Value}");
            }
            var role = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;

            var idEspaceClaim = User.Claims.FirstOrDefault(c => c.Type == "EspaceActifId")?.Value;
            var nomEspaceClaim = User.Claims.FirstOrDefault(c => c.Type == "EspaceActifNom")?.Value;

            if (role != "m_1" && idEspaceClaim == null)
                return BadRequest("Espace actif manquant pour utilisateur non-admin.");

            if (idEspaceClaim == null)
                return BadRequest("Aucun espace actif défini.");

            Console.WriteLine("Espace Actif ID depuis le token : " + idEspaceClaim);
            Connexion connect = new Connexion();
            var connex = connect.ConnectPostgres();
            // var result = _service.SearchUtilisateur(connex, searchTerm, int.Parse(idEspaceClaim));
            var result = _service.SearchDynamicUtilisateur(
                connex,
                searchTerm,
                role == "m_1" ? null : int.Parse(idEspaceClaim), // admin → pas de filtre
                role
            );
            return Ok(result);
        }

        [Authorize]
        [HttpGet("searchAllUser")]
        public async Task<IActionResult> SearchAllUser([FromQuery] string searchTerm)
        {
            Console.WriteLine("Claims reçues :");
            foreach (var c in User.Claims)
            {
                Console.WriteLine($"Type = {c.Type}  |  Value = {c.Value}");
            }

            var idEspaceClaim = User.Claims.FirstOrDefault(c => c.Type == "EspaceActifId")?.Value;
            var nomEspaceClaim = User.Claims.FirstOrDefault(c => c.Type == "EspaceActifNom")?.Value;


            if (idEspaceClaim == null)
                return BadRequest("Aucun espace actif défini.");

            Console.WriteLine("Espace Actif ID depuis le token : " + idEspaceClaim);
            Connexion connect = new Connexion();
            var connex = connect.ConnectPostgres();
            var result = _service.SearchAllUtilisateur(connex, searchTerm);
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
            Connexion connect = new Connexion();
            var connex = connect.ConnectPostgres();
            await _service.SynchroniserUtilisateursAsync();
            var utilisateurs = _service.GetAllUtilisateurs(connex); // <-- renvoyer la liste mise à jour
            return Ok(utilisateurs);
        }
    }
}

