using MessagerieInterneAPI.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MessagerieInterneAPI
{
    [ApiController]
    [Route("api/[controller]")]
    public class PoleController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;
        private readonly PoleService _service;
        private readonly Connexion _connexion;

        public PoleController(AppDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
            _context = context;
            _service = new PoleService(_context);
            _connexion = new Connexion();
        }

        // [Authorize(Roles = "m_1")]
        [HttpGet("getAllPole")]
        public async Task<IActionResult> GetAllPole()
        {
            var poles =  await _service.GetAllPole();
            return Ok(poles);
        }
    }
}
