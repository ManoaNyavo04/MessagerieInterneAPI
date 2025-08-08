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
        private LoginRequest login = new LoginRequest();

        public UtilisateurController(IConfiguration config, AppDbContext context, UtilisateurService service)
        {
            _config = config;
            _context = context;
            _service = service ?? throw new ArgumentNullException(nameof(service));
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

            var token = login.GenererToken(siUtilisateur, _config);
            var profilUtilisateur = _service.GetProfilUtilisateur(siUtilisateur);
            return Ok(new
            {
                token,
                profilUtilisateur
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
    }
}

