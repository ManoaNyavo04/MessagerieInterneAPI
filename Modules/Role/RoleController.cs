using MessagerieInterneAPI.Data;
using MessagerieInterneAPI.Entite;
using MessagerieInterneAPI.Modules.Role;
using Microsoft.AspNetCore.Mvc;

namespace MessagerieInterneAPI
{
    [ApiController]
    [Route("api/[controller]")]
    public class RoleController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;
        Connexion connect = new Connexion();
        private readonly RoleService _roleService;

        public RoleController(RoleService roleService)
        {
            _roleService = roleService;
        }

        [HttpGet("roles")]
        public async Task<IActionResult> GetAllRoles()
        {
            var connex = connect.ConnectPostgres();
            return Ok(_roleService.GetAllRoles(connex));
            
        }
    }
}
