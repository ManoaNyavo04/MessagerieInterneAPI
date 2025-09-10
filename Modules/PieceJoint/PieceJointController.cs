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
        private Connexion connexion = new Connexion();

        public PieceJointController(AppDbContext context, PieceJointService pieceJointeService)
        {
            _context = context;
            _pieceJointeService = pieceJointeService;
        }

        [HttpPost("envoyer-piece-jointe")]
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
        }


    }
}
