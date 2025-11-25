using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MessagerieInterneAPI
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ResetController : ControllerBase
    {
        private readonly ResetService _resetService;

        public ResetController(ResetService resetService)
        {
            _resetService = resetService;
        }

        [Authorize(Roles = "m_1")]
        [HttpPost("reset")]
        public async Task<IActionResult> ResetMessagerie()
        {
            var idUtilisateurClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            if (idUtilisateurClaim == null) return Unauthorized();

            int currentUserId = int.Parse(idUtilisateurClaim.Value);

            await _resetService.ResetMessagerieAsync(currentUserId);
            return NoContent();
        }
    }
}