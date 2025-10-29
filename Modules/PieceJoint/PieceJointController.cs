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

        [HttpPost("upload")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadFile([FromForm] PieceJointDTO dto)
        {
            if (dto.Fichier == null || dto.Fichier.Length == 0)
                return BadRequest("Aucun fichier reçu");

            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "Uploads");
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var nomOriginal = dto.Fichier.FileName;

            var fileName = Guid.NewGuid() + Path.GetExtension(nomOriginal);
            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await dto.Fichier.CopyToAsync(stream);
            }

            // Enregistrement dans la base
            await _pieceJointeService.AjouterPieceJointe(
                connexion.ConnectPostgres(),
                dto.IdMessage,
                dto.IdType,

                fileName,
                nomOriginal
            );

            return Ok(new { chemin = fileName, nom = nomOriginal });
        }


        //https://10.5.100.7:8888/api/Employe
        // BEARER_TOKEN= eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1bmlxdWVfbmFtZSI6IkxhbGFtYnlDYW50aW5lIiwiTG9naXN0aXF1ZSI6IkFwcGxpY2F0aW9uIiwibmJmIjoxNzM0MzQ3MjEyLCJleHAiOjE3NjU4ODMyMTIsImlhdCI6MTczNDM0NzIxMiwiaXNzIjoieW91dENvbXBhbnlJc3N1ZXIuY29tIiwiYXVkIjoieW91dENvbXBhbnlJc3N1ZXIuY29tIn0.3jJnu2fs_QjrACjBa20_KJCVKIoTaxdnctxBlv6wzBQ



    }
}
