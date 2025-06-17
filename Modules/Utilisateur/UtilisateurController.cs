using MessagerieInterneAPI.Data;
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
                return Unauthorized("Matricule ou mot de passe invalide.");
            }

            var token = login.GenererToken(siUtilisateur, _config);
            var profilUtilisateur = _service.GetProfilUtilisateur(siUtilisateur);
            return Ok(new
            {
                token,
                profilUtilisateur
            });
        }
    }
}
