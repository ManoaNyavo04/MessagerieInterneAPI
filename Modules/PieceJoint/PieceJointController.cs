using System.Net;
using MessagerieInterneAPI.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MessagerieInterneAPI
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class PieceJointController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly PieceJointService _pieceJointeService;
        private readonly IWebHostEnvironment _env;
        private Connexion connexion = new Connexion();

        public PieceJointController(AppDbContext context, PieceJointService pieceJointeService, IWebHostEnvironment env)
        {
            _context = context;
            _pieceJointeService = pieceJointeService;
            _env = env;
        }

        [Authorize]
        [HttpPost("upload")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadFile([FromForm] PieceJointDTO dto)
        {
            if (dto.Fichier == null || dto.Fichier.Length == 0)
                return BadRequest("Aucun fichier reçu");

            const long maxSize = 25 * 1024 * 1024; // 25 Mo
            if (dto.Fichier.Length > maxSize)
                return BadRequest("Le fichier dépasse la taille maximale autorisée de 25 Mo.");

            var nomOriginal = dto.Fichier.FileName;
            string fileName = await _pieceJointeService.UploadGeneric(dto, nomOriginal, "Uploads");

            return Ok(new { chemin = fileName, nom = nomOriginal });
        }

        [Authorize]
        [HttpGet("download/{id}")]
        public async Task<IActionResult> DownloadFile(int id)
        {
            var (bytes, contentType, fileName) = await _pieceJointeService.GetFileForDownload(id);
            if (bytes == null)
                return NotFound("Fichier introuvable.");
            return File(bytes, contentType, fileName);
        }
    }
}
