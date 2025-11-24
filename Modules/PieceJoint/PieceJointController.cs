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

        /*[HttpPost("envoyer-piece-jointe")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadPieceJointe([FromForm] PieceJointDTO dto)
        {
            if (dto.Fichier == null || dto.Fichier.Length == 0)
                return BadRequest("Aucun fichier reçu");

            var nomFichier = Path.GetFileName(dto.Fichier.FileName);
            var dossier = Path.Combine("Uploads", "PiecesJointes");
            Directory.CreateDirectory(dossier);
            var chemin = Path.Combine(dossier, nomFichier);

            using (var stream = new FileStream(chemin, FileMode.Create))
            {
                await dto.Fichier.CopyToAsync(stream);
            }

            var typeMime = dto.Fichier.ContentType;
            var taille = (int)dto.Fichier.Length;

            var idType = await _pieceJointeService.GetOrCreateType(connexion.ConnectPostgres(), typeMime, taille);
            await _pieceJointeService.AjouterPieceJointe(connexion.ConnectPostgres(), dto.IdMessage, idType, chemin);

            return Ok(new { chemin });
        }*/

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

        /*public async Task<IActionResult> UploadProfile([FromForm] PieceJointDTO dto)
        {
            if (dto.Fichier == null || dto.Fichier.Length == 0)
                return BadRequest("Aucun fichier reçu");


            var nomOriginal = dto.Fichier.FileName;
            string fileName = await _pieceJointeService.UploadGeneric(dto, nomOriginal, "profiles");

            return Ok(new { chemin = fileName, nom = nomOriginal });
        }*/


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
